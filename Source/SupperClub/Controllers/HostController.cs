using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using System.Web.Security;
using SupperClub.Domain;
using SupperClub.Models;
using SupperClub.Code;
using System.Web.Configuration;
using SupperClub.Web.Helpers;
using System.Text.RegularExpressions;
using System.Drawing;
using System.IO;
using SupperClub.Logger;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class HostController : BaseController
    {
        public HostController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }
        public ActionResult EditDetails(int supperClubId)
        {
            var model = _supperClubRepository.GetSupperClub(supperClubId);
            //if (UserMethods.IsLoggedIn)
            //{
            //  //  ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
            //    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(supperClubId, UserMethods.CurrentUser.Id);
            //}

            if (!string.IsNullOrEmpty(model.Twitter))
                model.Twitter = "@" + model.Twitter;

            return View("Details", model);
        }

        private List<object> GetReviews(Domain.SupperClub model)
        {
            var listReviews = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //List<Event> lstEvent = _supperClubRepository.GetAllEventsForASupperClub(model.Id).ToList();
            //List<Review> lstReviews = lstEvent.SelectMany(x => x.Reviews).OrderByDescending(y => y.DateCreated).ToList();
            IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews(0, model.Id).OrderByDescending(y => y.DateCreated).ToList();
            for (int i = 0; i < lstReviews.Count; i++)
            // for (int i=0; i<model.Reviews.Count();i++)
            {
                var tempReview = lstReviews[i];
                //  var tempReview = model.Reviews[i];
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
                        createdDate = tempReview.DateCreated.ToString("ddd, d MMM yyyy"),
                        rating = tempReview.Rating == null ? 0 : tempReview.Rating,
                        review = tempReview.PublicReview,
                        Name = guestName

                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return listReviews;
        }
        private List<object> GetImages(Domain.SupperClub model)
        {
            var listImages = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            foreach (SupperClubImage img in model.SupperClubImages)
                listImages.Add(new { imagePath = "SupperClubs/" + img.ImagePath });
                
            List<Event> lstEvent = _supperClubRepository.GetAllEventsForASupperClub(model.Id).OrderByDescending(x => x.Start).ToList<Event>();
            foreach (Event _event in lstEvent)
            {
                foreach (EventImage _eventImage in _event.EventImages)
                    listImages.Add(new { imagePath = "Events/" + _eventImage.ImagePath });                           
            }
            listImages = listImages.Distinct().ToList();

            return listImages;
        }
        
        private List<object> GetFutureEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            List<Event> lstEvents = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).OrderBy(x => x.Start).ToList<Event>();
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = model.AverageRank == null ? 0 : (int)model.AverageRank,//tempEvent.AverageRank== null?0 : tempEvent.AverageRank,
                    reviewsCount = model.NumberOfReviews,//tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0"
                });
            }
            return listEvents;
        }
        private List<object> GetPastEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            //for (int i = 0; i < model.PastEvents.Count; i++)
            List<Event> lstEvents = model.PastEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).OrderByDescending(x => x.Start).ToList<Event>();

            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0"
                });
            }
            return listEvents;
        }
        public JsonResult LoadDefaultData(int supperClubId)
        {

            var model = _supperClubRepository.GetSupperClub(supperClubId);

            return Json(new { futureEvents = GetFutureEvents(model), pastEvents = GetPastEvents(model), reviews = GetReviews(model),images = GetImages(model) }, JsonRequestBehavior.AllowGet);
        }
        private List<object> GetSeatingOptions(Event ofEvent)
        {
            var jsonSeating = new List<object>();
            if (ofEvent.MultiSeating)
            {
                foreach (EventSeating es in ofEvent.EventSeatings)
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
            return jsonSeating;
        }
        private List<object> GetMenuSeatingOption(Event ofEvent)
        {
            var jsonMenuOption = new List<object>();
            if (ofEvent.MultiMenuOption)
            {
                foreach (EventMenuOption em in ofEvent.EventMenuOptions)
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
            return jsonMenuOption;
        }

        public ActionResult Faqs()
        {
            return View();
        }
        public ActionResult WelComeHost()
        {
            if (UserMethods.IsLoggedIn)
            {
                //    if (SupperClub.Code.UserMethods.CurrentUser.SupperClubs.Count < 1)
                return RedirectToAction("Welcome");
                //    else
                //        return RedirectToAction("CreateEvent", "Event");
            }
            else
            { return RedirectToAction("LogOnHost", "Account", new { returnUrl = "/Host/WelComeHost" }); }
        }

        [Authorize]
        public ActionResult HostProfile(int supperClubId)
        {

            if (supperClubId == UserMethods.SupperClubId || UserMethods.IsAdmin)
            {
                HostDetailModel model = new HostDetailModel();

                SupperClub.Domain.SupperClub _supperClub = new SupperClub.Domain.SupperClub();
                _supperClub = _supperClubRepository.GetSupperClub(supperClubId);

                SupperClub.Domain.User _user = new SupperClub.Domain.User();
                _user = _supperClubRepository.GetUser(_supperClub.User.Id);
                model.user = _user;
                model.supperclub = _supperClub;

                Dictionary<string, int> HostStaus = new Dictionary<string, int>();
                HostStaus.Add(SupperClubStatus.Pro.ToString(), (int)SupperClubStatus.Pro);
                HostStaus.Add(SupperClubStatus.Amateur.ToString(), (int)SupperClubStatus.Amateur);
                HostStaus.Add(SupperClubStatus.DontKnow.ToString(), (int)SupperClubStatus.DontKnow);

                ViewBag.HostStaus = HostStaus;
                IList<ProfileTag> lstProfileTags = _supperClubRepository.GetProfileTags();

                if (UserMethods.IsLoggedIn && UserMethods.IsHost)
                {
                    if (!UserMethods.IsAdmin)
                        lstProfileTags = lstProfileTags.Where(t => t.TargetUser == (int)ProfileTagTargetUser.Host).ToList();
                }

                ViewBag.Tags = lstProfileTags;
                return View(model);
            }
            else
            {
              return  RedirectToAction("Index", "Home");                
            }
        }
        [HttpPost]
        [Authorize, ValidateInput(false)]
        public ActionResult HostProfile(HostDetailModel model)
        {

            Dictionary<string, int> HostStaus = new Dictionary<string, int>();
            HostStaus.Add(SupperClubStatus.Pro.ToString(), (int)SupperClubStatus.Pro);
            HostStaus.Add(SupperClubStatus.Amateur.ToString(), (int)SupperClubStatus.Amateur);
            HostStaus.Add(SupperClubStatus.DontKnow.ToString(), (int)SupperClubStatus.DontKnow);

            ViewBag.HostStaus = HostStaus;

            IList<ProfileTag> lstProfileTags = _supperClubRepository.GetProfileTags();

            if (UserMethods.IsLoggedIn && UserMethods.IsHost)
            {
                if (!UserMethods.IsAdmin)
                    lstProfileTags = lstProfileTags.Where(t => t.TargetUser == (int)ProfileTagTargetUser.Host).ToList();
            }
            if (model.supperclub.Id == UserMethods.SupperClubId || UserMethods.IsAdmin)
            {
            try
            {
                
                    //  User _user = UserMethods.CurrentUser;
                    User _user = _supperClubRepository.GetUser(model.user.Id);
                    _user.FirstName = model.user.FirstName;
                    _user.LastName = model.user.LastName;
                    _user.ContactNumber = model.user.ContactNumber;
                    bool userStatus = _supperClubRepository.UpdateUser(_user);

                    SupperClub.Domain.SupperClub _supperclub = _supperClubRepository.GetSupperClubForUser(model.user.Id);

                    //   _supperclub.Name = model.supperclub.Name;
                    _supperclub.Summary = model.supperclub.Summary;
                    _supperclub.Description = model.supperclub.Description;
                    _supperclub.Facebook = model.supperclub.Facebook;
                    _supperclub.Twitter = model.supperclub.Twitter;
                    _supperclub.Blog = model.supperclub.Blog;
                    _supperclub.Pinterest = model.supperclub.Pinterest;

                    _supperclub.Instagram = model.supperclub.Instagram;

                    _supperclub.Address = model.supperclub.Address;
                    _supperclub.Address2 = model.supperclub.Address2;
                    _supperclub.City = model.supperclub.City;
                    _supperclub.Country = model.supperclub.Country;
                    _supperclub.PostCode = model.supperclub.PostCode;
                    GeoLocation geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(model.supperclub.PostCode, model.supperclub.City, model.supperclub.Address, model.supperclub.Address2);
                    if (geolocation != null)
                    {
                        _supperclub.Longitude = geolocation.Longitude;
                        _supperclub.Latitude = geolocation.Latitude;
                    }

                    _supperclub.BankName = model.supperclub.BankName;
                    _supperclub.VATNumber = model.supperclub.VATNumber;
                    _supperclub.AccountNumber = model.supperclub.AccountNumber;
                    _supperclub.TradingName = model.supperclub.TradingName;
                    _supperclub.SortCode = model.supperclub.SortCode;

                    _supperclub.BankAddress = model.supperclub.BankAddress;

                    //Create a list of images to be deleted
                    string[] stringSeparators = new string[] { "," };
                    string[] imagePaths = model.supperclub.ImagePaths.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    var imagesToBeDeleted = from x in _supperclub.SupperClubImages
                                            where (!imagePaths.Any(val => x.ImagePath.Contains(val)))
                                            select x;

                    string[] images = null;
                    if (!string.IsNullOrEmpty(model.supperclub.ImagePaths))
                    {
                        images = model.supperclub.ImagePaths.Trim().TrimEnd(',').Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        model.supperclub.ImagePath = images[images.Length - 1];
                    }
                    _supperclub.ImagePaths = model.supperclub.ImagePaths;
                    _supperclub.ImagePath = model.supperclub.ImagePath;

                    bool statuschange = false;
                    int OldStatus = 0;
                    if (_supperclub.Status != model.supperclub.Status)
                    {
                        statuschange = true;
                        OldStatus = _supperclub.Status;
                        _supperclub.Status = model.supperclub.Status;
                    }
                    _supperclub.SupperClubProfileTagList = model.supperclub.SupperClubProfileTagList;


                    bool supperclubStatus = _supperClubRepository.UpdateSupperClub(_supperclub);
                    if (supperclubStatus)
                    {
                        SetNotification(NotificationType.Success, "Your settings  has been saved successfully.");
                        LogMessage("Updated SupperClub Details: " + model.supperclub.Id.ToString());
                        if (images != null && images.Length > 0)
                            MoveImage(images);

                        //Physically delete files from server                
                        if (imagesToBeDeleted != null)
                        {
                            try
                            {
                                foreach (SupperClubImage t in imagesToBeDeleted)
                                {
                                    string imagePathName = Server.MapPath(WebConfigurationManager.AppSettings["SupperClubImagePath"]) + t.ImagePath;
                                    if (System.IO.File.Exists(imagePathName))
                                    {
                                        System.IO.File.Delete(imagePathName);
                                    }
                                    else
                                    {
                                        string imagePathNameTemp = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]) + t.ImagePath;
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

                    }
                    else
                    {
                        LogMessage("Problem Updating SupperClub Details: " + model.supperclub.Id.ToString(), LogLevel.ERROR);
                        SetNotification(Models.NotificationType.Error, "There was a problem updating your settings. Please try again.", true);
                    }

                    if (OldStatus == model.supperclub.Status)
                    {
                        SupperclubStatusChangeLog status = new SupperclubStatusChangeLog();
                        status.OldStatus = OldStatus;
                        status.Status = model.supperclub.Status;
                        status.SupperClubId = _supperclub.Id;
                        status.UserId = UserMethods.CurrentUser.Id;
                        status.CreatedDate = DateTime.Now;

                        SupperclubStatusChangeLog _status = _supperClubRepository.AddSupperclubStatusChangeLog(status);
                    }

                    ViewBag.Tags = lstProfileTags;
                    return RedirectToAction("HostProfile", "Host", new { supperClubId = model.supperclub.Id });
                }
                catch (Exception ex)
                {
                    SetNotification(NotificationType.Error, "Your settings are not saved -" + ex.Message, true, false);
                    return RedirectToAction("HostProfile", "Host", new { supperClubId = model.supperclub.Id });
                }
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        
        }
        public ActionResult Details(int supperClubId)
        {
            var model = _supperClubRepository.GetSupperClub(supperClubId);
            
            if (!string.IsNullOrEmpty(model.Twitter))
                model.Twitter = "@" + model.Twitter;

            return View("Details", model);
        }

        public ActionResult Details1(int supperClubId)
        {
            SupperClubModel vmodel = new SupperClubModel();
            var model = _supperClubRepository.GetSupperClub(supperClubId);

            if (model.Active)
            {
                ViewBag.NextGrubClub = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).OrderBy(x => x.Start).First();
                ViewBag.FollowersCount = _supperClubRepository.GetSupperClubFollowers(model.Id);


                if (UserMethods.IsLoggedIn)
                {
                    //  ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
                    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(supperClubId, UserMethods.CurrentUser.Id);
                    if (model.FutureEvents != null)
                        ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(ViewBag.NextGrubClub.Id, UserMethods.CurrentUser.Id);

                }

                if (!string.IsNullOrEmpty(model.Twitter))
                    model.Twitter = "@" + model.Twitter;
                vmodel.SupperClub = model;
                if (model.FutureEvents != null && model.FutureEvents.Count > 0)
                {
                    ViewBag.NextEventId = ViewBag.NextGrubClub.Id.ToString();
                    ViewBag.Lattitude = ViewBag.NextGrubClub.Latitude;
                    ViewBag.Longitude = ViewBag.NextGrubClub.Longitude;
                }
                if (model.SupperClubImages.Count() < 10)
                {
                    vmodel.Images = new List<string>();
                    int imageCount = model.SupperClubImages.Count();
                    foreach (SupperClubImage img in model.SupperClubImages)
                        vmodel.Images.Add("SupperClubs/" + img.ImagePath);
                    List<Event> lstEvent = _supperClubRepository.GetAllEventsForASupperClub(model.Id).OrderByDescending(x => x.Start).ToList<Event>();
                    foreach (Event _event in lstEvent)
                    {
                        foreach (EventImage _eventImage in _event.EventImages)
                        {
                            vmodel.Images.Add("Events/" + _eventImage.ImagePath);
                            imageCount++;

                        }
                        if (imageCount > 10)
                            break;
                    }


                }
                IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews(0, model.Id).OrderByDescending(y => y.DateCreated).ToList();
                ViewBag.Reviews = lstReviews.OrderByDescending(a => a.DateCreated).Distinct().ToList<Review>();


                return View("Details1", vmodel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }


        public ActionResult DetailsByName(string hostname)
        {
            SupperClubModel vmodel = new SupperClubModel();
            var model = _supperClubRepository.GetSupperClub(hostname);

            if (model.Active)
            {
                if (model.FutureEvents != null && model.FutureEvents.Count > 0)
                {
                    ViewBag.NextGrubClub = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).OrderBy(x => x.Start).FirstOrDefault();
                    ViewBag.FutureEventCount = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).Count();

                }
                else
                {
                    ViewBag.FutureEventCount = 0;
                }
                ViewBag.FollowersCount = _supperClubRepository.GetSupperClubFollowers(model.Id);
                if (UserMethods.IsLoggedIn)
                {
                    //  ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
                    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(model.Id, UserMethods.CurrentUser.Id);
                    if (model.FutureEvents != null && model.FutureEvents.Count > 0)

                        ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(model.FutureEvents[0].Id, UserMethods.CurrentUser.Id);
                }
                if (!string.IsNullOrEmpty(model.Twitter))
                    model.Twitter = "@" + model.Twitter;
                vmodel.SupperClub = model;
                if (ViewBag.NextGrubClub != null)
                {
                    ViewBag.NextEventId = ViewBag.NextGrubClub.Id.ToString();
                    ViewBag.Lattitude = ViewBag.NextGrubClub.Latitude;
                    ViewBag.Longitude = ViewBag.NextGrubClub.Longitude;
                }
                if (model.SupperClubImages.Count() < 10)
                {
                    vmodel.Images = new List<string>();
                    int imageCount = model.SupperClubImages.Count();
                    foreach (SupperClubImage img in model.SupperClubImages)
                        vmodel.Images.Add("SupperClubs/" + img.ImagePath);
                    List<Event> lstEvent = _supperClubRepository.GetAllEventsForASupperClub(model.Id).OrderByDescending(x => x.Start).ToList<Event>();
                    foreach (Event _event in lstEvent)
                    {
                        foreach (EventImage _eventImage in _event.EventImages)
                        {
                            vmodel.Images.Add("Events/" + _eventImage.ImagePath);
                            imageCount++;

                        }
                        if (imageCount > 10)
                            break;
                    }


                }
                IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews(0, model.Id).OrderByDescending(y => y.DateCreated).ToList();
                ViewBag.Reviews = lstReviews.OrderByDescending(a => a.DateCreated).Distinct().ToList<Review>();


                return View("Details1", vmodel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public ActionResult WorkingWithGrubclub()
        {
            return View();
        }
        public ActionResult WhatWeList()
        {
            return View();
        }
        public ActionResult SettingUpAGrubClub()
        {
            return View();
        }
        public ActionResult UploadingOntoGrubClub()
        {
            return View();
        }
        public ActionResult ReviewsByName(string hostname, bool loadSummary)
        {
            IList<Event> events = _supperClubRepository.GetAllEventsWithReview(hostname);
            SupperClubReviewModel model = new SupperClubReviewModel();
            model.SupperClub = _supperClubRepository.GetSupperClub(hostname);
            model.ShowSummary = loadSummary;
            model.Reviews = new List<ReviewList>();
            if (events != null)
            {
                foreach (Event e in events)
                {
                    if (e.Reviews != null && e.Reviews.Count > 0)
                    {
                        foreach (Review r in e.Reviews)
                        {
                            ReviewList rl = new ReviewList();
                            rl.EventId = e.Id;
                            rl.EventName = e.Name;
                            rl.EventUrlFriendlyName = e.UrlFriendlyName;
                            rl.ReviewDate = r.DateCreated;
                            rl.EventReview = r;
                            model.Reviews.Add(rl);
                        }
                    }
                }
            }
            return View("SupperClubReviews", model);
        }
        public ActionResult OurChefs()
        {
            List<SupperClub.Domain.SupperClub> model = _supperClubRepository.GetAllActiveSupperClubs().ToList();
            return View(model);
        }

        [Authorize]
        public ActionResult RegisterSupperClub()
        {
            if (UserMethods.CurrentUser.SupperClubs.Count > 0)
                return RedirectToAction("CreateEvent", "Event", new { supperClubId = UserMethods.CurrentUser.SupperClubs.First().Id });

            SupperClub.Domain.SupperClub model = new Domain.SupperClub();
            LogMessage("Started GC Registration");
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult RegisterSupperClub(SupperClub.Domain.SupperClub model)
        {
            LogMessage("Begin GC Registration Post");
            bool accountValidation = true;
            bool imageError = false;

            if(model != null && !string.IsNullOrEmpty(model.Name))
            {
                LogMessage("Begin GC Validation");
                int check = _supperClubRepository.IsExistSupperClub(model.Name, model.UserId);
                if(check > 0)
                {
                    string errorMsg = "";
                    LogMessage("Validation failed while GC Registration");
                    if(check == 1)
                        errorMsg = "There were problems registering your Grub Club, please fix them and try again. You already have a Grub Club profile registered with this e-mail account! ";
                    else if (check == 2)
                        errorMsg = "There were problems registering your Grub Club, please fix them and try again. A Grub Club profile with this name already exists! ";
                    
                    SetNotification(NotificationType.Error, errorMsg);
                    accountValidation = false;
                    return View(model);
                }
                else
                {
                    LogMessage("GC Validation successful");
                }
            }
            
            
            string[] images = null;
            if (!string.IsNullOrEmpty(model.ImagePaths) && model.ImagePaths.Length > 0 && (!model.ImagePaths.ToLower().Contains("error")))
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
                model.ImagePaths = null;
                imageError = true;
            }
            
            GeoLocation geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(model.PostCode, model.City, model.Address, model.Address2);
            if (geolocation == null)
                ModelState.AddModelError("Address", "We couldn't find your address on a map!");

            if (ModelState.IsValid && accountValidation)
            {
                model.UserId = (Guid)UserMethods.UserId;
                model.Active = true; //SC must be activated by an Admin

                model.ImagePath = string.IsNullOrEmpty(model.ImagePath) ? "defaultSupperClubImage.png" : model.ImagePath;

                model.Longitude = geolocation.Longitude;
                model.Latitude = geolocation.Latitude;
                
                if (model.Twitter != null)
                    model.Twitter = model.Twitter.Replace("@", "");

                Regex rgx = new Regex("[^a-zA-Z0-9 -]");
                model.UrlFriendlyName = rgx.Replace(model.Name, "").Trim().Replace(" ", "-").ToLower();
                model.DateCreated = DateTime.Now;

                SupperClub.Domain.SupperClub s = _supperClubRepository.RegisterSupperClub(model, UserMethods.CurrentUser.Email);

                if (s != null &&  s.Id > 0)
                {                   
                    //MoveImage(model.ImagePath);
                    if(images != null)
                        MoveImage(images);

                    LogMessage("Change User Role to GC Owner");
                    //AddToRole(Membership.GetUser(), "Host");
                    UserMethods.ClearSettingInSession("RoleType"); //So their new role will be activated

                    //Send email to Admin about new SC
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendAdminNewSupperClubRegisteredEmail(UserMethods.CurrentUser.FirstName, UserMethods.CurrentUser.aspnet_Users.LoweredUserName, s.Name);

                    LogMessage("Registered GC: " + s.Name);
                    if (!imageError)
                        SetNotification(NotificationType.Info, "Your Grub Club is registered! Now it's time to create your first event, please note this won't be searchable until someone has approved your Grub Club.", true);
                    else
                        SetNotification(NotificationType.Warning, "Your Grub Club is registered but there were errors uploading images! Please review your profile. Now it's time to create your first event, please note this won't be searchable until someone has approved your Grub Club.", true);

                    return RedirectToAction("CreateEvent", "Event", new { supperClubId = s.Id });
                }
                else
                {
                    LogMessage("Problems with GC Registration");
                    string errorMsg = "There were problems registering your Grub Club, please try again.";
                    
                    SetNotification(NotificationType.Error, errorMsg);
                    return View(model);
                }
            }
            else
            {
                LogMessage("Errors with GC Registration");
                string errorMsg = "There were problems registering your Grub Club, please fix them and try again.";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMsg = errorMsg + " " + error.ErrorMessage;
                    }
                }
                SetNotification(NotificationType.Error, errorMsg);
                return View(model);
            }

        }

        [Authorize]
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateSupperClub(SupperClub.Domain.SupperClub model)
        {
            bool imageError = false;
            //string newImagePath = null;
            //if (model.File != null)
            //{
            //    newImagePath = ImageScaler.ConvertImage(model.File);
            //    if (newImagePath != null)
            //        model.ImagePath = newImagePath; // only update the image if conversion was successful
            //}
            string[] images = null;
            if (!string.IsNullOrEmpty(model.ImagePaths) && model.ImagePaths.Length > 0 && (!model.ImagePaths.ToLower().Contains("error")))
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
                model.ImagePaths = null;
                if (!string.IsNullOrEmpty(model.ImagePaths) && model.ImagePaths.Length > 0)
                    imageError = true;
            }
            model.ImagePath = string.IsNullOrEmpty(model.ImagePath) ? "defaultEventImage.png" : model.ImagePath;

            GeoLocation geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(model.PostCode, model.City, model.Address, model.Address2);
            if (geolocation != null)
            {
                model.Longitude = geolocation.Longitude;
                model.Latitude = geolocation.Latitude;
            }

            if (model.Twitter != null)
                model.Twitter = model.Twitter.Replace("@", "");

            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            model.UrlFriendlyName = rgx.Replace(model.Name, "").Trim().Replace(" ", "-").ToLower();

            bool success = _supperClubRepository.UpdateSupperClub(model);
            if (images != null && images.Length > 0)
                MoveImage(images);

            if (success)
            {
                LogMessage("Updated GC Details" + model.Id.ToString());
                string notificationMessage = "Your Grub Club details have been updated.";


                //if (!string.IsNullOrEmpty(model.ImagePath))
                //    MoveImage(model.ImagePath);

                //if (model.File != null && newImagePath == null) // there was a file, but it didn't convert successfully
                //    SetNotification(NotificationType.Warning, "We updated your details, but there was a problem with your image file, please try again.", true);
                //else 
                if (geolocation == null)
                    SetNotification(NotificationType.Warning, "We updated your details, but we couldn't find your address on a map, please check it again.", true);
                if(imageError)
                    SetNotification(NotificationType.Warning, "We updated your details, but we couldn't update the images, please check it again.", true);
                else
                    SetNotification(Models.NotificationType.Success, notificationMessage, true);
            }
            else
            {
                LogMessage("Problem Updating Grub Club details: " + model.Id.ToString(), LogLevel.ERROR);
                SetNotification(Models.NotificationType.Error, "There was a problem updating your Grubclub. Please try again.", true);
            }

            return RedirectToAction("EditDetails", "Host", new { supperClubId = model.Id });
        }

        [Authorize(Roles = "Host")]
        public ActionResult Vouchers()
        {
            Voucher model = new Voucher();
            ViewBag.Events = _supperClubRepository.GetAllActiveEventsForASupperClub(UserMethods.SupperClubId.Value);
            //ViewBag.VoucherType = 
            return View(model);
        }

        [Authorize(Roles = "Host")]
        [HttpPost]
        public ActionResult Vouchers(Voucher model)
        {
            LogMessage("Started Voucher Creation");
            model.OwnerId = (int)VoucherOwner.Host;
            return View("Vouchers");
        }

        [Authorize(Roles = "Host")]
        public ActionResult UploadImages(IEnumerable<HttpPostedFileBase> files)
        {
            var result = new List<string>();

            foreach (HttpPostedFileBase file in files)
            {
                try
                {
                    //.. standard file upload code
                    String img = ImageScaler.ConvertImage(file);
                    if (img != null)
                        result.Add(img);
                    else
                        LogMessage("Problem Uploading Images for GrubClub.", LogLevel.ERROR);
                }
                catch (Exception ex)
                {
                    LogMessage("Problem Uploading Images for GrubClub. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
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


        private void MoveImage(string imageName)
        {
            string oldPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]) + imageName;
            string newPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["SupperClubImagePath"]) + imageName;
            if (System.IO.File.Exists(oldPathAndName))
            {
                System.IO.File.Copy(oldPathAndName, newPathAndName);
                System.IO.File.Delete(oldPathAndName);
            }
        }

        private void MoveImage(string[] imageNames)
        {
            foreach (string imageName in imageNames)
            {
                string oldPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]) + imageName;
                string newPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["SupperClubImagePath"]) + imageName;

                if (System.IO.File.Exists(oldPathAndName))
                {
                    System.IO.File.Copy(oldPathAndName, newPathAndName);
                    System.IO.File.Delete(oldPathAndName);
                }
            }
        }
        public JsonResult HostNameAvailable(string Name)
        {
            Domain.SupperClub _currentSupperClub = null;
            if (UserMethods.IsAdmin)
            {
                // Allows Admin to update other GC's. Assume's Admin will never duplicate name.
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            if (UserMethods.SupperClubId.HasValue)
            {
                _currentSupperClub = _supperClubRepository.GetSupperClub(UserMethods.SupperClubId.Value);
                // If GC owner hasn't changed the name then it's ok
                if (_currentSupperClub.Name == Name)
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
            Regex rgx = new Regex("[^a-zA-Z0-9 -]");
            string UrlFriendlyName = rgx.Replace(Name, "").Replace(" ", "-");
            var _supperClub = _supperClubRepository.GetSupperClub(UrlFriendlyName);
            if (_supperClub != null) // name already in use
                return Json(false, JsonRequestBehavior.AllowGet);

            return Json(true, JsonRequestBehavior.AllowGet);

        }

        [Authorize]
        [HttpPost]
        public JsonResult FollowChef(int supperClubId)
        {
            LogMessage("Started to add Chef to current user's favourite list");

            try
            {
                int cntFollowChef = _supperClubRepository.GetUserFavouriteSupperClubCount(UserMethods.CurrentUser.Id);
                bool status = _followChef(supperClubId);
                if (status)
                {
                    LogMessage("Added Grub Clubs to user's favourites.");
                    //Segment Tracking
                    SupperClub.Domain.SupperClub sc = _supperClubRepository.GetSupperClub(supperClubId);
                    bool hasFutureEvents = false;
                    int availableSeats = 0;
                    if(sc.FutureEvents != null && sc.FutureEvents.Count > 0)
                    {
                        foreach(Event e in sc.FutureEvents)
                        {
                            if (!e.Private && (!hasFutureEvents || availableSeats <= 0))
                            {
                                availableSeats += e.TotalNumberOfAvailableSeats;
                                hasFutureEvents = true;
                                if(hasFutureEvents && availableSeats > 0)
                                    break;
                            }
                        }
                    }
                    Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(),
                            "Follow a chef",
                            new Segment.Model.Properties() {{ "ChefProfileName", sc.Name },
                    { "CountOfOtherChefsBeingFollowed", cntFollowChef.ToString() },
                    { "NumberOfReviews", sc.NumberOfReviews.ToString()},
                    { "AverageStarRating", sc.AverageRank.ToString()},
                    { "HasFutureEvents", hasFutureEvents ? "Yes":"No" },
                    { "AreAllFutureEventsSoldout", availableSeats <= 0 ? "Yes":"No" },                    
                    { "FollowDateTime", DateTime.Now.ToString() }
                });   

                    return Json(new { status = status, message = "You are now following this chef!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Failed to add Grub Clubs to user's favourite Grub Club list.");
                    return Json(new { status = status, message = "Sorry, there was some problem. Please try again!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error while adding Grub Clubs to user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { status = false, message = "Sorry, there was some problem. Please try again!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult UnfollowChef(int supperClubId)
        {
            LogMessage("Started to remove Chef to current user's favourite list");

            try
            {
                bool status = _supperClubRepository.RemoveSupperClubFromFavourite(supperClubId, (Guid)SupperClub.Code.UserMethods.CurrentUser.Id);
                if (status)
                {
                    LogMessage("Removed Grub Clubs to user's favourites.");
                    return Json(new { status = status, message = "You have stopped following this chef!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Failed to remove Grub Clubs from user's favourite Grub Club list.");
                    return Json(new { status = status, message = "Sorry, there was some problem. Please try again!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error while removing Grub Clubs from user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { status = false, message = "Sorry, there was some problem. Please try again!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult FollowChef(int supperClubId, string rurl)
        {
            bool jr = _followChef(supperClubId);
            if (!string.IsNullOrEmpty(rurl))
                return Redirect(rurl);
            return RedirectToAction("Index", "Home");
        }

        #region Static Public Pages
        public ActionResult Welcome()
        {
            /*return View("Welcome"); */
            return View("Welcomev2");
            //return RedirectToAction("UnderMaintenance", "home");

        }
        public ActionResult Welcomev2()
        {
            return View();
        }
        public ActionResult Welcomev3()
        {
            return View();
        }
        [Authorize(Roles = "Host")]
        public ActionResult Tips()
        {
            return View("HostAdminTips");
        }

        #endregion
        #region Private Methods
        private bool _followChef(int supperClubId)
        {
            bool status = false;
            UserFavouriteSupperClub ufs = new UserFavouriteSupperClub();
            ufs.SupperClubId = supperClubId;
            ufs.UserId = (Guid)SupperClub.Code.UserMethods.CurrentUser.Id;

            try
            {
                status = _supperClubRepository.AddSupperClubToFavourite(ufs);
            }
            catch (Exception ex)
            {
                LogMessage("Error while adding Grub Clubs to user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }
            return status;
        }

        #endregion
    }
}
