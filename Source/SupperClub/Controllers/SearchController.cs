using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Models;
using SupperClub.Code;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using SupperClub.Web.Helpers;
using System.Globalization;
using SupperClub.Logger;
using System.Web.Caching;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class SearchController : BaseController
    {
        public SearchController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        public ActionResult Index()
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            if (Request.QueryString["keyword"] != null && !string.IsNullOrEmpty(Request.QueryString["keyword"].ToString()))
            {
                searchModel.SearchCriteria.FoodKeyword = Request.QueryString["keyword"].ToString();

            }
            if (Request.QueryString["source"] != null && !string.IsNullOrEmpty(Request.QueryString["source"].ToString()))
            {
                try
                {
                    int src = int.Parse(Request.QueryString["source"].ToString());
                    if (src == (int)SearchSourcePageType.Home)
                        searchModel.SearchCriteria.SourcePageTypeId = src;
                }
                catch
                {
                    searchModel.SearchCriteria.SourcePageTypeId = (int)SearchSourcePageType.Search;
                }
            }
            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.Cuisine = getCuisines();//_supperClubRepository.GetAvailableCuisines();
            searchModel.SearchCriteria.Tag = getTags();

            return View("SearchResult1", searchModel);
        }

        public ActionResult SearchResult()
        {
            // Just here for people trying to go to this page from their browser
            return RedirectToAction("Index");
        }

        public JsonResult AddToWaitListRegisteredUser(int? eventId, string source)
        {
            bool success = false;
            try
            {
                if (eventId == null || eventId == 0)
                {
                    LogMessage("AddToWaitListRegisteredUser: EventId null or zero. PageSource: "+ source + "  UserAgent: " + Request.UserAgent + " URL:" + Request.Url, LogLevel.ERROR);
                }
                else
                {
                    LogMessage("Adding User to Event Wait List. Event Id: " + eventId.ToString());

                        if (SupperClub.Code.UserMethods.IsLoggedIn)
                        {
                            SupperClub.Domain.EventWaitList ewl = new EventWaitList();
                            ewl.UserId = SupperClub.Code.UserMethods.UserId;
                            ewl.Email = SupperClub.Code.UserMethods.UserName;
                            ewl.AddedDate = DateTime.Now;
                            ewl.EventId = (int)eventId;
                            success = AddUserToWaitList(ewl);
                        }
                        if (success)
                        {
                            LogMessage("Added User to Event Wait List. Event Id: " + eventId.ToString() + " PageSource: " + source);
                            Event _event = _supperClubRepository.GetEvent((int)eventId);
                            if (_event != null)
                            {
                                Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Add To waitlist", new Segment.Model.Properties() {
                                { "EmailAddress", SupperClub.Code.UserMethods.CurrentUser.Email},
                                { "EventName", _event.Name },
                                { "EventDate", _event.Start.ToString() }});
                            }
                        }

                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to add user to Event Wait List. Event Id: " + eventId + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AddToWaitList(int? eventId, string email, string source)
        {
            ActionStatus actionStatus = new ActionStatus();
            if (eventId != null && eventId != 0 && email.Length > 0)
            {
                LogMessage("Adding User to Event Wait List. Event Id: " + eventId);
                try
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
                    if (validEmail)
                    {
                        SupperClub.Domain.EventWaitList ewl = new EventWaitList();
                        ewl.UserId = null;
                        ewl.Email = email;
                        ewl.EventId = (int)eventId;
                        ewl.AddedDate = DateTime.Now;
                        actionStatus.Success = AddUserToWaitList(ewl);
                        if (!actionStatus.Success)
                        {
                            LogMessage("Failed to add to WaitList.", LogLevel.ERROR);
                            actionStatus.NotificationMessage = "There was a problem adding to waitlist, please try again or contact an Administrator.";
                        }
                        else
                        {
                            LogMessage("Added user to WaitList. Event Id: " + eventId + "  Email:" + email, LogLevel.INFO);
                            actionStatus.NotificationMessage = "You have been added to the Waitlist.";
                            Event _event = _supperClubRepository.GetEvent((int)eventId);
                            if (_event != null)
                            {
                                Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Add To waitlist", new Segment.Model.Properties() {
                                { "EmailAddress", SupperClub.Code.UserMethods.CurrentUser.Email},
                                { "EventName", _event.Name },
                                { "EventDate", _event.Start.ToString() }});
                            }
                        }
                    }
                    else
                    {
                        LogMessage("Add to WaitList : E-mail Id entered is not valid.", LogLevel.ERROR);
                        actionStatus.NotificationMessage = "E-mail address entered is not valid. Please check and try again.";
                    }

                }
                catch (Exception ex)
                {
                    LogMessage("Failed to add user to Event Wait List. Event Id: " + eventId + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                }
            }
            else
            {
                actionStatus.NotificationMessage = "There was a problem adding to waiting list, please try again or contact an Administrator.";
                if (eventId == null || eventId == 0)
                    LogMessage("Null or zero Event Id passed. PageSource: " + source + " UserAgent: " + Request.UserAgent + " URL:" + Request.Url, LogLevel.ERROR);
                else
                    LogMessage("Blank e-mail address passed. PageSource: " + source + " UserAgent: " + Request.UserAgent + " URL:" + Request.Url, LogLevel.ERROR);
            }

            return Json(actionStatus, JsonRequestBehavior.AllowGet);
        }

        bool AddUserToWaitList(EventWaitList ewl)
        {
            int wlStatus = 0;
            bool success = false;
            try
            {
                wlStatus = _supperClubRepository.AddToWaitList(ewl);
                switch (wlStatus)
                {
                    case 0:
                        LogMessage("Failed to add user to Event Wait List. Event Id: " + ewl.EventId);
                        break;
                    case 1:
                        LogMessage("Added User to Event Wait List. Event Id: " + ewl.EventId);
                        break;
                    case 2:
                        LogMessage("User was already on the Wait List. Event Id: " + ewl.EventId);
                        break;
                    default:
                        LogMessage("Failed to add user to Event Wait List. Event Id: " + ewl.EventId);
                        break;
                }

                // Send notification e-mail to admin
                if (wlStatus != 2)
                {
                    EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                    success = emailer.SendAdminWaiListEmail(ewl.EventId, ewl.Email);
                }
                else
                {
                    success = true;
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to add user to Event Wait List. Event Id: " + ewl.EventId + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }
            return success;
        }
        public ActionResult CategoryDetailsByName(string categoryname)
        {
            LogMessage("Showing the category page");
            SearchCategoryModel searchCategoryModel = new SearchCategoryModel();
            searchCategoryModel.CategoryUrlFriendlyName = categoryname;
            searchCategoryModel.CategoryTags = _supperClubRepository.GetSearchCategoryTags(categoryname);

            return View("Category", searchCategoryModel);
        }

        public ActionResult TagSearchResult(string tagname)
        {
            LogMessage("Searching for Events with Tag");
            //Check if the tag is part of a category, if yes then redirect else continue
            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            if (tag != null && tag.Id > 0)
            {
                SearchCategoryTag sct = _supperClubRepository.GetSearchCategoryTagByTagId(tag.Id);
                if(sct != null)
                    return RedirectToActionPermanent("CategorySearchResult", "Search", new { categoryname = sct.SearchCategory.UrlFriendlyName, tagname = sct.Tag.UrlFriendlyName });
            }
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = string.Empty;
            searchModel.SearchCriteria.QueryArea = string.Empty;

            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id);

            return View("TagSearchResult1", searchModel);
        }
        public ActionResult TagCityAreaSearchResult(string categoryname, string tagname, string cityname, string areaname)
        {
            LogMessage("Searching for Events with Tag city and area");
            //Check if the tag is part of a category, if yes then redirect else continue
            bool isValidCombo = _supperClubRepository.CheckTagCategoryCombinationValidity(categoryname.ToLower(), tagname.ToLower());
            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            if (!isValidCombo && tag != null && tag.Id > 0)
            {
                SearchCategoryTag sct = _supperClubRepository.GetSearchCategoryTagByTagId(tag.Id);
                if (sct != null)
                    return RedirectToActionPermanent("TagCityAreaSearchResult", "Search", new { categoryname = sct.SearchCategory.UrlFriendlyName, tagname = sct.Tag.UrlFriendlyName, cityname = cityname, areaname = areaname });
            }

            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = cityname;
            searchModel.SearchCriteria.QueryArea = areaname;

            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id);

            return View("TagSearchResult1", searchModel);
        }
        public ActionResult TagCitySearchResult(string categoryname, string tagname, string cityname)
        {
            LogMessage("Searching for Events with Tag and city");
            //Check if the tag is part of a category, if yes then redirect else continue
            bool isValidCombo = _supperClubRepository.CheckTagCategoryCombinationValidity(categoryname.ToLower(), tagname.ToLower());
            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            if (!isValidCombo && tag != null && tag.Id > 0)
            {
                SearchCategoryTag sct = _supperClubRepository.GetSearchCategoryTagByTagId(tag.Id);
                if (sct != null)
                    return RedirectToActionPermanent("TagCitySearchResult", "Search", new { categoryname = sct.SearchCategory.UrlFriendlyName, tagname = sct.Tag.UrlFriendlyName, cityname = cityname });
            }

            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = cityname;
            searchModel.SearchCriteria.QueryArea = string.Empty;

            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id);

            return View("TagSearchResult1", searchModel);
        }
        public ActionResult CategorySearchResult(string categoryname, string tagname)
        {
            LogMessage("Searching for Events with Tag and city");
            //Check if the tag is part of a category, if yes then redirect else continue
            bool isValidCombo = _supperClubRepository.CheckTagCategoryCombinationValidity(categoryname.ToLower(), tagname.ToLower());
            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            if (!isValidCombo && tag != null && tag.Id > 0)
            {
                SearchCategoryTag sct = _supperClubRepository.GetSearchCategoryTagByTagId(tag.Id);
                if (sct != null)
                    return RedirectToActionPermanent("CategorySearchResult", "Search", new { categoryname = sct.SearchCategory.UrlFriendlyName, tagname = sct.Tag.UrlFriendlyName});
            }
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = "";
            searchModel.SearchCriteria.QueryArea = "";

            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id); // _supperClubRepository.GetAvailableCuisines();
            return View("TagSearchResult1", searchModel);
        }

        public ActionResult CategoryCitySearchResult(string categoryname, string tagname, string cityname)
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryArea = "";
            if (cityname != null && cityname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryCity = cityname.ToString();
            else
                searchModel.SearchCriteria.QueryCity = "";
            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id); // _supperClubRepository.GetAvailableCuisines();
            return View("TagCitySearchResult1", searchModel);
        }
        public ActionResult CategoryCityAreaSearchResult(string categoryname, string tagname, string cityname, string areaname)
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();

            if (cityname != null && cityname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryCity = cityname.ToString();
            else
                searchModel.SearchCriteria.QueryCity = "";
            if(string.IsNullOrEmpty(searchModel.SearchCriteria.QueryCity))
                searchModel.SearchCriteria.QueryArea = "";
            else
            {
                if (areaname != null && areaname.ToString().Length > 0)
                    searchModel.SearchCriteria.QueryArea = areaname.ToString();
                else
                    searchModel.SearchCriteria.QueryArea = "";
            }
            if (tagname != null && tagname.ToString().Length > 0)
                searchModel.SearchCriteria.QueryTag = tagname.ToString();
            else
                searchModel.SearchCriteria.QueryTag = "";

            Tag tag = _supperClubRepository.GetTagByName(tagname.ToLower());
            searchModel.SearchTag = tag;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(tag.Id); // _supperClubRepository.GetAvailableCuisines();
            return View("TagCitySearchResult1", searchModel);
        }
        public ActionResult SearchCityAreaResult(string cityname, string areaname)
        {
            LogMessage("Searched for CityArea Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = cityname;
            searchModel.SearchCriteria.QueryArea = areaname;

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0); // _supperClubRepository.GetAvailableCuisines();
            return View("SearchResult1", searchModel);
        }
        public ActionResult SearchCityResult(string cityname)
        {
            LogMessage("Searched for City Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.QueryCity = cityname;
            searchModel.SearchCriteria.QueryArea = "";

            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0); // _supperClubRepository.GetAvailableCuisines();
            return View("SearchResult1", searchModel);
        }

        [HttpPost]
        public ActionResult HomeSearchResultNew(HomePage model)
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();
            if (model.HomeSearch.StartDate == null)
                model.HomeSearch.StartDate = DateTime.Now;
            searchModel.SearchCriteria = model.HomeSearch;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0);
            return View("SearchResult", searchModel);
        }

        [HttpPost]
        public ActionResult HomeSearchResult(NewHome model)
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();
            if (model == null)
            {
                model = new NewHome();
                model.Location = "London";
            }
            SearchModel sm = new SearchModel();
            sm.Location = model.Location;
            sm.Guests = 2;
            if (!string.IsNullOrEmpty(model.DateString))
            {
                //Separator
                string[] fieldSeparators = new string[] { " - " };
                string[] fields = model.DateString.Split(fieldSeparators, StringSplitOptions.None);
                try
                {
                    DateTime StartDate = new DateTime();
                    DateTime EndDate = new DateTime();
                    bool parseStartDate = string.IsNullOrEmpty(fields[0]) ? false : DateTime.TryParse(fields[0], out StartDate);
                    if (!parseStartDate)
                    {
                        StartDate = DateTime.Now;
                        LogMessage("Could not parse StartDate. Input DateString:" + model.DateString, LogLevel.WARN);
                    }

                    bool parseEndDate = string.IsNullOrEmpty(fields[1]) ? false : DateTime.TryParse(fields[1], out EndDate);
                    if (!parseEndDate)
                    {
                        EndDate = StartDate.AddDays(100);
                        LogMessage("Could not parse EndDate. Input DateString:" + model.DateString, LogLevel.WARN);
                    }

                    int dayOffset = (EndDate - StartDate).Days;
                    sm.StartDate = StartDate;
                    sm.EndDateOffset = dayOffset == 0 ? 1 : dayOffset;
                }
                catch
                {
                    LogMessage("Could not parse StartDate or EndDate. Input DateString:" + model.DateString, LogLevel.WARN);
                    sm.StartDate = DateTime.Now;
                    sm.EndDateOffset = 100;
                }
            }
            else
            {
                LogMessage("Home Search: Blank Input DateString. Used default range.", LogLevel.WARN);
                sm.StartDate = DateTime.Now;
                sm.EndDateOffset = 100;
            }

            //if (model.HomeSearch.StartDate == null)
            //    model.HomeSearch.StartDate = DateTime.Now;
            searchModel.SearchCriteria = sm;
            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0);
            return View("SearchResult", searchModel);
        }

        [HttpPost]
        public ActionResult SearchResultJson(SearchModel searchCriteria)
        {
            LogMessage("Filtered Results");
            decimal minPrice = 0;
            decimal maxPrice = 1000000;
            PriceRange priceRange = new PriceRange();

            GeoLocation geolocation = null;
            //if (!string.IsNullOrEmpty(searchCriteria.Location))
            //    geolocation = Utils.GetAddressCoordinates(string.Empty, searchCriteria.Location);

            Search search = new Search();
            SearchFilter filter = new SearchFilter();
            filter.DietIds = new List<int>();
            filter.PriceRangeIds = new List<int>();
            filter.CuisineIds = new List<int>();
            filter.TagIds = new List<int>();
            //if (searchCriteria.PriceRangeOptions != null && searchCriteria.PriceRangeOptions.Length > 0)
            //{
            //    priceRange = _supperClubRepository.GetMinMaxPriceRange(searchCriteria.PriceRangeOptions);
            //    search.MinPrice = priceRange.MinPrice;
            //    search.MaxPrice = priceRange.MaxPrice;
            //}
            //else
            //{
            //    search.MinPrice = minPrice;
            //    search.MaxPrice = maxPrice;
            //}
            if (searchCriteria.DietOptions != null && searchCriteria.DietOptions.Length > 0)
                filter.DietIds = searchCriteria.DietOptions.ToList<int>();
            if (searchCriteria.AllergyFriendly)
            {
                List<int> allergyDiets = new List<int>();
                allergyDiets = _supperClubRepository.GetAllAllergyDiets();
                filter.DietIds.AddRange(allergyDiets);
            }
            if (searchCriteria.CuisineOptions != null && searchCriteria.CuisineOptions.Length > 0)
                filter.CuisineIds = searchCriteria.CuisineOptions.ToList<int>();

            try
            {
                search.Distance = int.Parse(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsSearchDistance"]);
            }
            catch
            {
                LogMessage("Could not fetch GoogleMapsSearchDistance from config. Used default value.", LogLevel.ERROR);
                search.Distance = 10;
            }

            if (geolocation != null)
            {
                search.Latitude = geolocation.Latitude;
                search.Longitude = geolocation.Longitude;
            }

            search.Guests = searchCriteria.Guests == null ? 0 : (int)searchCriteria.Guests;
            search.Diet = string.Empty;
            search.FoodKeyword = string.IsNullOrEmpty(searchCriteria.FoodKeyword) ? string.Empty : searchCriteria.FoodKeyword;
            //search.Charity = searchCriteria.Charity;
            //search.Alcohol = searchCriteria.Alcohol;
            //search.EndDateOffset = searchCriteria.EndDateOffset;
            //search.StartDate = searchCriteria.StartDate.HasValue ? searchCriteria.StartDate.Value : DateTime.Now;
            search.QueryTag = string.IsNullOrEmpty(searchCriteria.QueryTag) ? string.Empty : searchCriteria.QueryTag;
            search.QueryCity = string.IsNullOrEmpty(searchCriteria.QueryCity) ? string.Empty : searchCriteria.QueryCity;
            if (string.IsNullOrEmpty(search.QueryCity))
                search.QueryArea = string.Empty;
            else
                search.QueryArea = string.IsNullOrEmpty(searchCriteria.QueryArea) ? string.Empty : searchCriteria.QueryArea;
            var result = _supperClubRepository.SearchEvent(search, filter);
            SearchResultWithMetaData displayResults = new SearchResultWithMetaData();
            displayResults.SearchResults = result.OrderBy(x => x.EventDateTime).ToList();
            displayResults.SearchResultCount = result.Count;
            displayResults.SearchLocation = geolocation;

            bool setThisWeekTitle = false;
            bool setNextWeekTitle = false;
            bool setThisMonthTitle = false;
            bool setLaterTitle = false;
            string[] fieldSeparators = new string[] { " " };
            CultureInfo cultureGB = CultureInfo.GetCultureInfo("en-GB");
            foreach (SearchResult sr in displayResults.SearchResults)
            {
                sr.EventPostCode = sr.EventPostCode.Split(fieldSeparators, StringSplitOptions.None)[0];
                //DateTime eventStartDate = DateTime.Parse(sr.EventDate, cultureGB);
                //sr.EventDate = eventStartDate.ToString("ddd, d MMM yyyy");
                //if (!setThisWeekTitle && eventStartDate <= DateTime.Now.AddDays(7))
                //{
                //    sr.ShowListingTitle = true;
                //    sr.ListingTitle = "This Week's Grub Clubs";
                //    setThisWeekTitle = true;
                //}
                //if (!setNextWeekTitle && eventStartDate > DateTime.Now.AddDays(7) && eventStartDate <= DateTime.Now.AddDays(14))
                //{
                //    sr.ShowListingTitle = true;
                //    sr.ListingTitle = "Next Week's Grub Clubs";
                //    setNextWeekTitle = true;
                //}
                //if (!setThisMonthTitle && eventStartDate > DateTime.Now.AddDays(14) && eventStartDate <= DateTime.Now.AddDays(30))
                //{
                //    sr.ShowListingTitle = true;
                //    sr.ListingTitle = "Later this month";
                //    setThisMonthTitle = true;
                //}
                //if (!setLaterTitle && eventStartDate > DateTime.Now.AddDays(30))
                //{
                //    sr.ShowListingTitle = true;
                //    sr.ListingTitle = "Grub Clubs coming your way in the next couple of months";
                //    setLaterTitle = true;
                //}
                if (sr.EventDescription.Length > 250)
                {
                    bool linkFound = false;
                    if (sr.EventDescription.Substring(0, 250).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</a>").Count))
                    {
                        sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                    }
                    else
                    {
                        int indexer = System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count;
                        int requiredStringLength = Utils.findStringNthIndex(sr.EventDescription, "</a>", indexer);
                        sr.EventDescription = sr.EventDescription.Substring(0, requiredStringLength + 4) + "...";
                        linkFound = true;
                    }
                    if (sr.EventDescription.Substring(0, 250).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</strong>").Count))
                    {
                        if (!linkFound)
                            sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                    }
                    else
                    {
                        if (!linkFound)
                            sr.EventDescription = sr.EventDescription.Substring(0, 250) + "</strong>...";
                        else
                            sr.EventDescription = sr.EventDescription + "</strong>...";
                    }
                }
            }

            return Json(displayResults, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Index1()
        {
            LogMessage("Searched for Events");
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            //if (Request.QueryString["keyword"] != null && !string.IsNullOrEmpty(Request.QueryString["keyword"].ToString()))
            //{
            //    searchModel.SearchCriteria.FoodKeyword = Request.QueryString["keyword"].ToString();
            //}
            //if (Request.QueryString["source"] != null && !string.IsNullOrEmpty(Request.QueryString["source"].ToString()))
            //{
            //    try
            //    {
            //        int src = int.Parse(Request.QueryString["source"].ToString());
            //        if (src == (int)SearchSourcePageType.Home)
            //            searchModel.SearchCriteria.SourcePageTypeId = src;
            //    }
            //    catch
            //    {
            //        searchModel.SearchCriteria.SourcePageTypeId = (int)SearchSourcePageType.Search;
            //    }
            //}-
            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();

            searchModel.SearchCriteria.Cuisine = getCuisines();// _supperClubRepository.GetAvailableCuisines();
            searchModel.SearchCriteria.Tag = getTags();

            return View("SearchResult", searchModel);
        }

        [HttpPost]
        public ActionResult GeAlltCusines(string tag = null)
        {
            List<Cuisine> lstCuisines = new List<Cuisine>();
            lstCuisines = getCuisines().ToList<Cuisine>();
            var lstObjs = new List<object>();
            for (int i = 0; i < lstCuisines.Count; i++)
            {
                lstObjs.Add(new
                {
                    Id=lstCuisines[i].Id,
                    Name=lstCuisines[i].Name
               });
            }

            return Json(lstObjs, JsonRequestBehavior.AllowGet);

        }
        private IList<Cuisine> getCuisines(int tagId = 0)
        {
            List<Cuisine> lstCuisines= new List<Cuisine>();


            string key = "Cuisines" + DateTime.Now.Day;
           // if(key.Contains(
            if (System.Web.HttpRuntime.Cache[key] != null)
            {
                lstCuisines = (List<Cuisine>) System.Web.HttpRuntime.Cache[key];
            }
            else
            {

                lstCuisines = _supperClubRepository.GetAvailableCuisines(tagId).ToList<Cuisine>();
                System.Web.HttpRuntime.Cache.Insert(key, lstCuisines, null, DateTime.Now.AddDays(1), TimeSpan.Zero);
            }

            return lstCuisines;


        }

        private IList<Tag> getTags(int tagId = 0)
        {
            List<Tag> lstTags = new List<Tag>();


            string key = "Tags" + DateTime.Now.Day;

            string keyAdmin = "TagAdmin";

            // if(key.Contains(

            if (UserMethods.IsLoggedIn && UserMethods.IsAdmin)
            {
                if (System.Web.HttpRuntime.Cache[keyAdmin] != null)
                {
                    lstTags = (List<Tag>)System.Web.HttpRuntime.Cache[keyAdmin];
                }
                else
                {

                    lstTags = _supperClubRepository.GetActiveEventTags().ToList<Tag>();
                    lstTags = lstTags.OrderBy(t => t.Name).ToList<Tag>();

                    System.Web.HttpRuntime.Cache.Insert(keyAdmin, lstTags, null, DateTime.Now.AddDays(1), TimeSpan.Zero);
                }
            }
            else
            {
                if (System.Web.HttpRuntime.Cache[key] != null)
                {
                    lstTags = (List<Tag>)System.Web.HttpRuntime.Cache[key];
                }
                else
                {

                    lstTags = _supperClubRepository.GetActiveEventTags().ToList<Tag>();
                    lstTags = lstTags.OrderBy(t => t.Name).ToList<Tag>();

                        lstTags = lstTags.Where(t => t.Private == false).ToList<Tag>();

                    System.Web.HttpRuntime.Cache.Insert(key, lstTags, null, DateTime.Now.AddDays(1), TimeSpan.Zero);
                }
            }

            return lstTags;


        }

        public ActionResult SearchResult1()
        {
            // Just here for people trying to go to this page from their browser
            return RedirectToAction("Index1");
        }
        public ActionResult RedirectSearch()
        {
            // Just here for people trying to go to this page from their browser
            return RedirectPermanent("/pop-up-restaurants");
        }
        public ActionResult RedirectSearch1()
        {
            // Just here for people trying to go to this page from their browser
            return RedirectPermanent("/pop-up-restaurants");
        }
        [HttpPost]
        public ActionResult SearchResult1_Init(string tag = null, string city = null, string area = null)
        {
            LogMessage("Searched for Events");
            string cacheKey = "ListingInit";
            SearchViewModel searchModel = new SearchViewModel();
            searchModel.SearchResult = new List<SearchResult>();
            searchModel.SearchCriteria = new SearchModel();

            searchModel.SearchCriteria.Location = "";
            searchModel.SearchCriteria.Guests = 2;
            searchModel.SearchCriteria.StartDate = DateTime.Now;

            searchModel.SearchCriteria.AllergyFriendly = false;
            searchModel.SearchCriteria.Diet = _supperClubRepository.GetStandardDiets();
            searchModel.SearchCriteria.PriceRange = _supperClubRepository.GetPriceRange();
            if (!string.IsNullOrEmpty(tag))
            {
                searchModel.SearchCriteria.QueryTag = tag;
                Tag _tag = _supperClubRepository.GetTagByName(tag.ToLower());
                searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(_tag.Id);
                if (cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += tag;
            }
            else
                searchModel.SearchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0);

            if (!string.IsNullOrEmpty(city))
            {
                searchModel.SearchCriteria.QueryCity = city;
                if(cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += city;
            }
            if (!string.IsNullOrEmpty(area))
            {
                searchModel.SearchCriteria.QueryArea = area;
                if (cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += area;
            }

            Search search = new Search();
            SearchFilter sf = new SearchFilter();
            SearchResultWithMetaData displayResults = new SearchResultWithMetaData();

            if (System.Web.HttpRuntime.Cache[cacheKey] != null)
            {
                displayResults = (SearchResultWithMetaData)System.Web.HttpRuntime.Cache[cacheKey];



            }
            else
            {

                if (SupperClub.Code.UserMethods.IsLoggedIn)
                {
                    searchModel.SearchCriteria.UserId = SupperClub.Code.UserMethods.UserId;
                }
                else
                {
                    searchModel.SearchCriteria.UserId = null;
                }
                decimal minPrice = 0;
                decimal maxPrice = 1000000;
                PriceRange priceRange = new PriceRange();

                GeoLocation geolocation = null;
                //if (!string.IsNullOrEmpty(searchModel.SearchCriteria.Location))
                //    geolocation = Utils.GetAddressCoordinates(string.Empty, searchModel.SearchCriteria.Location);

                sf.DietIds = new List<int>();
                sf.PriceRangeIds = new List<int>();
                sf.CuisineIds = new List<int>();
                sf.TagIds = new List<int>();
                //if (searchModel.SearchCriteria.PriceRangeOptions != null && searchModel.SearchCriteria.PriceRangeOptions.Length > 0)
                //{
                //    priceRange = _supperClubRepository.GetMinMaxPriceRange(searchModel.SearchCriteria.PriceRangeOptions);
                //    search.MinPrice = priceRange.MinPrice;
                //    search.MaxPrice = priceRange.MaxPrice;
                //}
                //else
                //{
                //search.MinPrice = minPrice;
                //search.MaxPrice = maxPrice;
                //}
                if (searchModel.SearchCriteria.DietOptions != null && searchModel.SearchCriteria.DietOptions.Length > 0)
                    sf.DietIds = searchModel.SearchCriteria.DietOptions.ToList<int>();
                if (searchModel.SearchCriteria.AllergyFriendly)
                {
                    List<int> allergyDiets = new List<int>();
                    allergyDiets = _supperClubRepository.GetAllAllergyDiets();
                    sf.DietIds.AddRange(allergyDiets);
                }
                if (searchModel.SearchCriteria.CuisineOptions != null && searchModel.SearchCriteria.CuisineOptions.Length > 0)
                    sf.CuisineIds = searchModel.SearchCriteria.CuisineOptions.ToList<int>();

                if (searchModel.SearchCriteria.TagOptions != null && searchModel.SearchCriteria.TagOptions.Length > 0)
                    sf.TagIds = searchModel.SearchCriteria.TagOptions.ToList<int>();



                try
                {
                    search.Distance = int.Parse(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsSearchDistance"]);
                }
                catch
                {
                    LogMessage("Could not fetch GoogleMapsSearchDistance from config. Used default value.", LogLevel.ERROR);
                    search.Distance = 10;
                }

                if (geolocation != null)
                {
                    search.Latitude = geolocation.Latitude;
                    search.Longitude = geolocation.Longitude;
                }

                search.Guests = searchModel.SearchCriteria.Guests == null ? 0 : (int)searchModel.SearchCriteria.Guests;
                search.Diet = string.Empty;
                search.FoodKeyword = string.IsNullOrEmpty(searchModel.SearchCriteria.FoodKeyword) ? string.Empty : searchModel.SearchCriteria.FoodKeyword;
                //search.Charity = searchModel.SearchCriteria.Charity;
                //search.Alcohol = searchModel.SearchCriteria.Alcohol;
                //search.EndDateOffset = searchModel.SearchCriteria.EndDateOffset;
                //search.StartDate = DateTime.Now;
                searchModel.SearchCriteria.StartDate = searchModel.SearchCriteria.StartDate.HasValue ? searchModel.SearchCriteria.StartDate.Value : DateTime.Now;
                search.QueryTag = string.IsNullOrEmpty(searchModel.SearchCriteria.QueryTag) ? string.Empty : searchModel.SearchCriteria.QueryTag;
                search.QueryCity = string.IsNullOrEmpty(searchModel.SearchCriteria.QueryCity) ? string.Empty : searchModel.SearchCriteria.QueryCity;
                if (string.IsNullOrEmpty(search.QueryCity))
                    search.QueryArea = string.Empty;
                else
                    search.QueryArea = string.IsNullOrEmpty(searchModel.SearchCriteria.QueryArea) ? string.Empty : searchModel.SearchCriteria.QueryArea;
                var result = _supperClubRepository.SearchEvent(search, sf);

                displayResults.SearchResults = result.OrderBy(x => x.EventDateTime).ToList();
                displayResults.SearchResultCount = result.Count;
                displayResults.SearchLocation = geolocation;

                string[] fieldSeparators = new string[] { " " };
                CultureInfo cultureGB = CultureInfo.GetCultureInfo("en-GB");
                foreach (SearchResult sr in displayResults.SearchResults)
                {
                    sr.EventPostCode = sr.EventPostCode.Split(fieldSeparators, StringSplitOptions.None)[0];
                    //DateTime eventStartDate = DateTime.Parse(sr.EventDate, cultureGB);
                    if (sr.Cost == "0.00")
                        sr.Cost = "Free";
                    //sr.EventDate = eventStartDate.ToString("ddd, d MMM yyyy");

                    if (sr.EventDescription.Length > 250)
                    {
                        bool linkFound = false;
                        if (sr.EventDescription.Substring(0, 250).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</a>").Count))
                        {
                            sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                        }
                        else
                        {
                            int indexer = System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count;
                            int requiredStringLength = Utils.findStringNthIndex(sr.EventDescription, "</a>", indexer);
                            sr.EventDescription = sr.EventDescription.Substring(0, requiredStringLength + 4) + "...";
                            linkFound = true;
                        }
                        if (sr.EventDescription.Substring(0, 250).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</strong>").Count))
                        {
                            if (!linkFound)
                                sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                        }
                        else
                        {
                            if (!linkFound)
                                sr.EventDescription = sr.EventDescription.Substring(0, 250) + "</strong>...";
                            else
                                sr.EventDescription = sr.EventDescription + "</strong>...";
                        }
                    }
                }

                System.Web.HttpRuntime.Cache.Insert(cacheKey, displayResults, null, DateTime.Now.AddMinutes(60), TimeSpan.Zero);

                if (sf.DietIds.Count > 0)
                    displayResults.SearchResults = displayResults.SearchResults.Where(x => sf.DietIds.Contains(x.DietId)).ToList<SearchResult>();
                if (sf.CuisineIds.Count > 0)
                    displayResults.SearchResults = displayResults.SearchResults.Where(x => sf.CuisineIds.Contains(x.CuisineId)).ToList<SearchResult>();

                if (sf.TagIds != null && sf.TagIds.Count > 0)
                    displayResults.SearchResults = displayResults.SearchResults.Where(x => sf.TagIds.Contains(x.TagId)).ToList<SearchResult>();


                //min price filter
                if (searchModel.SearchCriteria.MinPrice != null)
                    displayResults.SearchResults = (from s in displayResults.SearchResults
                                                where s.CostToGuest >= searchModel.SearchCriteria.MinPrice
                                select s).Distinct().ToList<SearchResult>();

                // max price filter
                if (searchModel.SearchCriteria.MaxPrice != null)
                    displayResults.SearchResults = (from s in displayResults.SearchResults
                                                    where s.CostToGuest <= searchModel.SearchCriteria.MaxPrice
                                                    select s).Distinct().ToList<SearchResult>();

                // BYOB filter
                if (searchModel.SearchCriteria.Alcohol != null)
                    displayResults.SearchResults = (from s in displayResults.SearchResults
                                                    where s.Byob == searchModel.SearchCriteria.Alcohol
                                    select s).Distinct().ToList<SearchResult>();

                // charity filter
                if (searchModel.SearchCriteria.Charity != null)
                    displayResults.SearchResults = (from s in displayResults.SearchResults
                                                    where s.Charity == searchModel.SearchCriteria.Charity
                                    select s).Distinct().ToList<SearchResult>();
                // date filetr
                if (searchModel.SearchCriteria.StartDate != null)
                {
                    DateTime _startDate = new DateTime();
                    DateTime _endDate = new DateTime();
                    _startDate = (DateTime)searchModel.SearchCriteria.StartDate;
                    _endDate = _startDate.AddDays(searchModel.SearchCriteria.EndDateOffset);
                    displayResults.SearchResults = (from s in displayResults.SearchResults
                                    where s.EventDateTime >= _startDate && s.EventDateTime <= _endDate
                                    select s).Distinct().ToList<SearchResult>();
                }
                displayResults.SearchResults = displayResults.SearchResults.OrderBy(x => x.EventDateTime).ToList();
                displayResults.SearchResultCount = displayResults.SearchResults.Count;
            }

            displayResults.SearchResults = displayResults.SearchResults.GroupBy(x => x.EventId).Select(x => x.First()).ToList<SearchResult>();

            return Json(displayResults, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SearchResult1(SearchModel searchCriteria)
        {
            LogMessage("Filtered Results");
            decimal minPrice = 0;
            decimal maxPrice = 1000000;
            PriceRange priceRange = new PriceRange();

            GeoLocation geolocation = null;
            //if (!string.IsNullOrEmpty(searchCriteria.Location))
            //    geolocation = Utils.GetAddressCoordinates(string.Empty, searchCriteria.Location);

            Search search = new Search();
            SearchFilter filter = new SearchFilter();
            filter.DietIds = new List<int>();
            filter.PriceRangeIds = new List<int>();
            filter.CuisineIds = new List<int>();

            if (SupperClub.Code.UserMethods.IsLoggedIn)
            {
                searchCriteria.UserId = SupperClub.Code.UserMethods.UserId;
            }
            else
            {
                searchCriteria.UserId = null;
            }
            //if (searchCriteria.PriceRangeOptions != null && searchCriteria.PriceRangeOptions.Length > 0)
            //{
            //    //priceRange = _supperClubRepository.GetMinMaxPriceRange(searchCriteria.PriceRangeOptions);
            //    search.MinPrice = searchCriteria.PriceRangeOptions[0];
            //    search.MaxPrice = searchCriteria.PriceRangeOptions[1];
            //}
            //else
            //{
            //    search.MinPrice = minPrice;
            //    search.MaxPrice = maxPrice;
            //}
            if (searchCriteria.DietOptions != null && searchCriteria.DietOptions.Length > 0)
                filter.DietIds = searchCriteria.DietOptions.ToList<int>();
            if (searchCriteria.AllergyFriendly)
            {
                List<int> allergyDiets = new List<int>();
                allergyDiets = _supperClubRepository.GetAllAllergyDiets();
                filter.DietIds.AddRange(allergyDiets);
            }
            if (searchCriteria.CuisineOptions != null && searchCriteria.CuisineOptions.Length > 0)
                filter.CuisineIds = searchCriteria.CuisineOptions.ToList<int>();

            if (searchCriteria.TagOptions != null && searchCriteria.TagOptions.Length > 0)
                filter.TagIds = searchCriteria.TagOptions.ToList<int>();



            try
            {
                search.Distance = searchCriteria.Distance <= 0 ? 10 : searchCriteria.Distance; //int.Parse(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsSearchDistance"]);
            }
            catch
            {
                LogMessage("Could not fetch GoogleMapsSearchDistance from config. Used default value.", LogLevel.ERROR);
                search.Distance = 10;
            }

            if (geolocation != null)
            {
                search.Latitude = geolocation.Latitude;
                search.Longitude = geolocation.Longitude;
            }

            search.Guests = searchCriteria.Guests == null ? 2 : (int)searchCriteria.Guests;
            search.Diet = string.Empty;
            search.FoodKeyword = string.IsNullOrEmpty(searchCriteria.FoodKeyword) ? string.Empty : searchCriteria.FoodKeyword;
            //search.Charity = searchCriteria.Charity;
            //search.Alcohol = searchCriteria.Alcohol;
            searchCriteria.EndDateOffset = searchCriteria.EndDateOffset <= 0 ? 1 : searchCriteria.EndDateOffset;

            //search.StartDate = DateTime.Now;
            searchCriteria.StartDate = searchCriteria.StartDate.HasValue ? searchCriteria.StartDate.Value : DateTime.Now;
            string cacheKey = "Listing";
            if (!string.IsNullOrEmpty(searchCriteria.QueryTag))
            {
                search.QueryTag = searchCriteria.QueryTag;
                Tag _tag = _supperClubRepository.GetTagByName(search.QueryTag.ToLower());
                searchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(_tag.Id);
                if (cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += search.QueryTag;
            }
            else
            {
                searchCriteria.Cuisine = _supperClubRepository.GetAvailableCuisines(0);
                search.QueryTag = string.Empty;
            }

            if (!string.IsNullOrEmpty(searchCriteria.QueryCity))
            {
                search.QueryCity = searchCriteria.QueryCity;
                if (cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += search.QueryCity;
            }
            else
            {
                search.QueryCity = string.Empty;
            }
            if (string.IsNullOrEmpty(search.QueryCity))
                search.QueryArea = string.Empty;
            else
                search.QueryArea = string.IsNullOrEmpty(searchCriteria.QueryArea) ? string.Empty : searchCriteria.QueryArea;
            if (!string.IsNullOrEmpty(search.QueryArea))
            {
                if (cacheKey.Length > 0)
                    cacheKey += "_";
                cacheKey += search.QueryArea;
            }
            if (searchCriteria.PriceRangeOptions != null && searchCriteria.PriceRangeOptions.Length > 0)
            {
                searchCriteria.MinPrice = searchCriteria.PriceRangeOptions[0];
                searchCriteria.MaxPrice = searchCriteria.PriceRangeOptions[1];
            }
            if (searchCriteria.PageNumber == 1)
                AddSearchtoLog(searchCriteria);

            //TODO: Ad logic to apply filter on cache
            SearchResultWithMetaData displayResults = new SearchResultWithMetaData();
            if (System.Web.HttpRuntime.Cache[cacheKey] != null && string.IsNullOrEmpty(searchCriteria.FoodKeyword))
            {
                SearchResultWithMetaData result = (SearchResultWithMetaData)System.Web.HttpRuntime.Cache[cacheKey];
                result = (SearchResultWithMetaData)System.Web.HttpRuntime.Cache[cacheKey];
                if (result != null || result.SearchResults != null && result.SearchResults.Count <= 0)
                {
                    System.Web.HttpRuntime.Cache.Remove(cacheKey);
                    RedirectToAction("SearchResult1", searchCriteria);
                }

                if (result != null)
                {
                    IList<SearchResult> searchResult = result.SearchResults;
                    if (filter.DietIds.Count > 0)
                        searchResult = searchResult.Where(x => filter.DietIds.Contains(x.DietId)).ToList<SearchResult>();
                    if (filter.CuisineIds.Count > 0)
                        searchResult = searchResult.Where(x => filter.CuisineIds.Contains(x.CuisineId)).ToList<SearchResult>();
                    if (filter.TagIds != null && filter.TagIds.Count > 0)
                        searchResult = searchResult.Where(x => filter.TagIds.Contains(x.TagId)).ToList<SearchResult>();

                    //min price filter
                    if (searchCriteria.MinPrice != null)
                    searchResult = (from s in searchResult
                                    where s.CostToGuest >= searchCriteria.MinPrice
                                    select s).Distinct().ToList<SearchResult>();

                    //max price filter
                    if (searchCriteria.MaxPrice != null)
                        searchResult = (from s in searchResult
                                        where s.CostToGuest <= searchCriteria.MaxPrice
                                        select s).Distinct().ToList<SearchResult>();

                    // BYOB and charity filter
                    if(searchCriteria.Alcohol != null)
                        searchResult = (from s in searchResult
                                    where s.Byob == searchCriteria.Alcohol
                                    select s).Distinct().ToList<SearchResult>();
                    if (searchCriteria.Charity != null)
                        searchResult = (from s in searchResult
                                    where s.Charity == searchCriteria.Charity
                                    select s).Distinct().ToList<SearchResult>();

                    if (searchCriteria.NewChef != null)
                        searchResult = (from s in searchResult
                                        where s.BrandNew== searchCriteria.NewChef
                                        select s).Distinct().ToList<SearchResult>();

                    //Keyword filter
                    //if (searchCriteria.FoodKeyword != null)
                    //    searchResult = (from s in searchResult
                    //                    where s.Byob == searchCriteria.Alcohol
                    //                    select s).Distinct().ToList<SearchResult>();

                    // Date Filter
                    if (searchCriteria.StartDate != null)
                    {
                        DateTime _startDate = new DateTime();
                        DateTime _endDate = new DateTime();
                        _startDate = (DateTime)searchCriteria.StartDate;
                        _endDate = _startDate.AddDays(searchCriteria.EndDateOffset);
                        searchResult = (from s in searchResult
                                        where s.EventDateTime >= _startDate && s.EventDateTime <= _endDate
                                        select s).Distinct().ToList<SearchResult>();
                    }
                    displayResults.SearchResults = searchResult.OrderBy(x => x.EventDateTime).ToList();
                    displayResults.SearchResultCount = searchResult.Count;
                    displayResults.SearchLocation = geolocation;
                }
            }
            else
            {
                var result = _supperClubRepository.SearchEvent(search, filter);
                displayResults.SearchResults = result.OrderBy(x => x.EventDateTime).ToList();
                displayResults.SearchResultCount = result.Count;
                displayResults.SearchLocation = geolocation;



                string[] fieldSeparators = new string[] { " " };
                CultureInfo cultureGB = CultureInfo.GetCultureInfo("en-GB");

                foreach (SearchResult sr in displayResults.SearchResults)
                {
                    sr.EventPostCode = sr.EventPostCode.Split(fieldSeparators, StringSplitOptions.None)[0];
                   // DateTime eventStartDate = DateTime.Parse(sr.EventDate, cultureGB);
                   //sr.EventDate = eventStartDate.ToString("ddd, d MMM yyyy");

                    if (sr.EventDescription.Length > 250)
                    {
                        bool linkFound = false;
                        if (sr.EventDescription.Substring(0, 250).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</a>").Count))
                        {
                            sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                        }
                        else
                        {
                            int indexer = System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<a").Count;
                            int requiredStringLength = Utils.findStringNthIndex(sr.EventDescription, "</a>", indexer);
                            sr.EventDescription = sr.EventDescription.Substring(0, requiredStringLength + 4) + "...";
                            linkFound = true;
                        }
                        if (sr.EventDescription.Substring(0, 250).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(sr.EventDescription.Substring(0, 250), "</strong>").Count))
                        {
                            if (!linkFound)
                                sr.EventDescription = sr.EventDescription.Substring(0, 250) + "...";
                        }
                        else
                        {
                            if (!linkFound)
                                sr.EventDescription = sr.EventDescription.Substring(0, 250) + "</strong>...";
                            else
                                sr.EventDescription = sr.EventDescription + "</strong>...";
                        }
                    }
                }
                 if (string.IsNullOrEmpty(searchCriteria.FoodKeyword))
                 {
                     SearchResultWithMetaData cacheResults = new SearchResultWithMetaData();
                     cacheResults.SearchResults = new List<SearchResult>();
                     foreach (SearchResult sr in displayResults.SearchResults)
                     {
                         cacheResults.SearchResults.Add(sr);
                     }
                     // cacheResults.SearchResults= displayResults.SearchResults;
                     System.Web.HttpRuntime.Cache.Insert(cacheKey, cacheResults, null, DateTime.Now.AddMinutes(60), TimeSpan.Zero);
                 }

                    if (filter.DietIds.Count > 0)
                        displayResults.SearchResults = displayResults.SearchResults.Where(x => filter.DietIds.Contains(x.DietId)).ToList<SearchResult>();
                    if (filter.CuisineIds.Count > 0)
                        displayResults.SearchResults = displayResults.SearchResults.Where(x => filter.CuisineIds.Contains(x.CuisineId)).ToList<SearchResult>();
                    if (filter.TagIds != null && filter.TagIds.Count > 0)
                        displayResults.SearchResults = displayResults.SearchResults.Where(x => filter.TagIds.Contains(x.TagId)).ToList<SearchResult>();


                    //min price filter
                    if (searchCriteria.MinPrice != null)
                        displayResults.SearchResults = (from s in displayResults.SearchResults
                                        where s.CostToGuest >= searchCriteria.MinPrice
                                        select s).Distinct().ToList<SearchResult>();

                    //max price filter
                    if (searchCriteria.MaxPrice != null)
                        displayResults.SearchResults = (from s in displayResults.SearchResults
                                        where s.CostToGuest <= searchCriteria.MaxPrice
                                        select s).Distinct().ToList<SearchResult>();

                    // BYOB filter
                    if (searchCriteria.Alcohol != null)
                        displayResults.SearchResults = (from s in displayResults.SearchResults
                                                        where s.Byob == searchCriteria.Alcohol
                                                        select s).Distinct().ToList<SearchResult>();
                    // Charity filter
                    if (searchCriteria.Charity != null)
                        displayResults.SearchResults = (from s in displayResults.SearchResults
                                                        where s.Charity == searchCriteria.Charity
                                                        select s).Distinct().ToList<SearchResult>();
                    // Date filetr
                    //if (searchCriteria.StartDate != null)
                    //displayResults.SearchResults = (from s in displayResults.SearchResults
                    //                                where DateTime.ParseExact(s.EventDate, "ddd, d MMM yyyy", CultureInfo.InvariantCulture) >= searchCriteria.StartDate && DateTime.ParseExact(s.EventDate, "ddd, d MMM yyyy", CultureInfo.InvariantCulture) <= ((DateTime)(searchCriteria.StartDate)).AddDays(searchCriteria.EndDateOffset)
                    //                                select s).Distinct().ToList<SearchResult>();
                    // date filetr
                    if (searchCriteria.StartDate != null)
                    {
                        DateTime _startDate = new DateTime();
                        DateTime _endDate = new DateTime();
                        _startDate = (DateTime)searchCriteria.StartDate;
                        _endDate = _startDate.AddDays(searchCriteria.EndDateOffset);
                        displayResults.SearchResults = (from s in displayResults.SearchResults
                                                        where s.EventDateTime >= _startDate && s.EventDateTime <= _endDate
                                                        select s).Distinct().ToList<SearchResult>();
                    }
                    displayResults.SearchResults = displayResults.SearchResults.OrderBy(x => x.EventDateTime).ToList();
                    displayResults.SearchResultCount = displayResults.SearchResults.Count;

                    if (!string.IsNullOrEmpty(searchCriteria.FoodKeyword))
                    {
                        Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Keyword search", new Segment.Model.Properties() {
                        { "SearchTerm", searchCriteria.FoodKeyword },
                        { "NumberOfResults", displayResults.SearchResultCount },
                        { "SearchDate", DateTime.Now.ToString() }});
                    }
            }

            displayResults.SearchResults = displayResults.SearchResults.GroupBy(x => x.EventId).Select(x => x.First()).ToList<SearchResult>();
            return Json(displayResults, JsonRequestBehavior.AllowGet);


        }

        private void AddSearchtoLog(SearchModel searchCriteria)
        {
            try
            {

                SearchLogDetail searchLog = new SearchLogDetail();
                searchLog.Byob = searchCriteria.Alcohol;
                searchLog.Charity = searchCriteria.Charity;
                searchLog.CreatedDate = DateTime.Now;
                searchLog.Cusine = (searchCriteria.CuisineOptions) == null ? null : string.Join("','", searchCriteria.CuisineOptions);
                searchLog.Diet = (searchCriteria.DietOptions) == null ? null : string.Join(",", searchCriteria.DietOptions);
                searchLog.Distance = searchCriteria.Distance;
                searchLog.EndDate = DateTime.Now.AddDays(searchCriteria.EndDateOffset);
                searchLog.Keyword = searchCriteria.FoodKeyword;
                searchLog.Postcode = searchCriteria.Location;
                searchLog.Price = (searchCriteria.PriceRangeOptions) == null ? null : string.Join(",", searchCriteria.PriceRangeOptions);
                searchLog.StartDate = searchCriteria.StartDate;
                searchLog.SourcePageTypeId = searchCriteria.SourcePageTypeId;
                if ((SupperClub.Code.UserMethods.CurrentUser) != null)
                    searchLog.UserId = SupperClub.Code.UserMethods.CurrentUser.Id;

                SearchLogDetail _searchLog = _supperClubRepository.CreateSeacrchLogDetail(searchLog);
                Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Filter by Search", new Segment.Model.Properties() {
                    { "Price", searchLog.Price  },
                    { "DateRange", searchLog.StartDate.ToString() + " - " + searchLog.EndDate.ToString() },
                    { "Cuisines", searchCriteria.SelectedCuisines },
                    { "Diets", searchCriteria.SelectedDiets },
                    { "Tags", searchCriteria.SelectedTags},
                    { "BYOB", searchLog.Byob == null ? string.Empty : searchLog.Byob.ToString() },
                    { "Charity", searchLog.Charity == null ? string.Empty : searchLog.Charity.ToString() },
                    { "SearchDate", DateTime.Now.ToString() }
                });
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);

            }
        }

    }
}
