using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SupperClub.Domain;
using System.Web.Security;
using SupperClub.Code;
using SupperClub.Data;
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
using System.Xml;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Braintree;
using Braintree.Exceptions;
using System.Net.Mail;
using System.Net.Sockets;
using System.Runtime.Serialization;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class BasicAuthenticationAttribute : ActionFilterAttribute
    {
        protected SupperClub.Code.SimplerAES sa = new SupperClub.Code.SimplerAES();
        private SupperClubRepository _supperClubRepository = new SupperClubRepository();

        public string BasicRealm { get; set; }

        public BasicAuthenticationAttribute(string basicRealm)
        {
            this.BasicRealm = basicRealm;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            var auth = req.Headers["Authorization"];

            if (!String.IsNullOrEmpty(auth))
            {
                string[] authHeader = auth.Split(' ');
                if (authHeader.Length > 1)
                    auth = authHeader[authHeader.Length - 1];
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(auth)).Split(':');
                var user = new { Name = cred[0], Pass = cred[1] };
                Guid? _userId = null;
                if (!string.IsNullOrEmpty(user.Name))
                    _userId = new Guid(sa.Decrypt(user.Name));
                if (_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    User _user = null;
                    _user = _supperClubRepository.GetUser((Guid)_userId);
                    
                    if (_user != null)
                    {
                        if (!UserMethods.IsSpecificUserLockedOut(_user.Email) && Membership.ValidateUser(_user.Email, user.Pass) && Roles.IsUserInRole(_user.aspnet_Users.UserName, "Feed"))
                            return;
                    }
                }
            }
            filterContext.HttpContext.Response.AddHeader("WWW-Authenticate", String.Format("Basic realm=\"{0}\"", BasicRealm ?? "GrubClub"));
            filterContext.Result = new HttpUnauthorizedResult();
        }
    }

    public class FeedOutPutFormatter : ActionFilterAttribute
    {
        private String[] _actionParams;

        // for deserialization
        public FeedOutPutFormatter(params String[] parameters)
        {
            this._actionParams = parameters;
        }

        // SERIALIZE
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //if (!(filterContext.Result is ViewResult)) return;

            // SETUP
            UTF8Encoding utf8 = new UTF8Encoding(false);
            HttpRequestBase request = filterContext.RequestContext.HttpContext.Request;
            string contentType = request.ContentType ?? string.Empty;
            try
            {
                ViewResult view = (ViewResult)(filterContext.Result);
                var data = view.ViewData.Model;
                // JSON
                if (contentType.Contains("text/xml") || contentType.Contains("application/xml"))
                {
                    // MemoryStream to encapsulate as UTF-8 (default UTF-16)
                    // http://stackoverflow.com/questions/427725/
                    //
                    // MemoryStream also used for atomicity but not here
                    // http://stackoverflow.com/questions/486843/
                    using (MemoryStream stream = new MemoryStream(500))
                    {
                        using (var xmlWriter =
                            XmlTextWriter.Create(stream,
                                new XmlWriterSettings()
                                {
                                    OmitXmlDeclaration = true,
                                    Encoding = utf8,
                                    Indent = true
                                }))
                        {

                            new DataContractSerializer(
                                data.GetType(),
                                null, // knownTypes
                                65536, // maxItemsInObjectGraph
                                false, // ignoreExtensionDataObject
                                true, // preserveObjectReference - overcomes cyclical reference issues
                                null // dataContractSurrogate
                                ).WriteObject(stream, data);
                        }

                        filterContext.Result = new ContentResult
                        {
                            ContentType = "application/xml",
                            Content = utf8.GetString(stream.ToArray()),
                            ContentEncoding = utf8
                        };
                    }
                }
                else //(contentType.Contains("application/json") || request.IsAjaxRequest())
                {
                    using (var stream = new MemoryStream())
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();

                        string content = js.Serialize(data);
                        filterContext.Result = new ContentResult
                        {
                            ContentType = "application/json",
                            Content = content,
                            ContentEncoding = utf8
                        };
                    }
                }
            }
            catch
            {
                return;
            }            
        }
    }

    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class FeedController : BaseController
    {
        public SupperClub.Code.SimplerAES sa = new SupperClub.Code.SimplerAES();

        public FeedController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
       // [Authorize(Roles = "Feed")]
        public JsonResult getUserId(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    LogMessage("Feed getUserId: Error. Email or Password was not passed or blank. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "Error: Email address was not passed or blank." }, JsonRequestBehavior.AllowGet);
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
                    LogMessage("Feed getUserId: Error. Could not find email address. Email address not registered. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "Could not find email address. Email address not registered" }, JsonRequestBehavior.AllowGet);
                }
                // If we get this far the username definitely exists
                if (UserMethods.IsSpecificUserLockedOut(username))
                {
                    LogMessage("Feed getUserId: Error. User account has been locked. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "User account has been locked" }, JsonRequestBehavior.AllowGet);
                }
        
                User u = _supperClubRepository.GetUser(email);
                LogMessage("Feed getUserId: User details fetched successfully. Email:" + email, LogLevel.INFO);
                return Json(new
                {
                    Email = u.Email,
                    UserId = sa.Encrypt(u.Id.ToString())                    
                }, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                LogMessage("Feed getUserId: Error fetching user id. Email:" + email + "  ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while fetching user id." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [FeedOutPutFormatter]
        [BasicAuthenticationAttribute("GrubClub")]
        public ActionResult getEvents(string postCode = null)
        {
            try
            {
                LogMessage("Feed Request: Get Event Listings.");

                EventList eventListing = new EventList();
                List<EventListOutput> eventListOutput = new List<EventListOutput>();
                if (!string.IsNullOrEmpty(postCode))
                    eventListing.PostCode = postCode;
                else
                    eventListing.PostCode = "";
                var result = _supperClubRepository.SearchEvents(eventListing);
                if (result == null || result.Count == 0)
                {
                    LogMessage("Event Listing: No result returned.", LogLevel.INFO);
                    ViewData.Model = "No Events Found.";
                    return View();
                }
                else
                {
                    foreach (EventListResult el in result)
                    {
                        EventListOutput elo = new EventListOutput();
                        elo.Chef = el.Chef;
                        elo.Cost = el.Cost;
                        elo.Currency = el.Currency;
                        elo.EventStart = el.EventStart;
                        elo.EventEnd = el.EventEnd;
                        elo.EventPostCode = el.EventPostCode;
                        elo.Soldout = el.Soldout;
                        elo.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/","") + el.EventImage;
                        elo.EventURL = ServerMethods.ServerUrl + el.EventUrlFriendlyName + "/" + el.EventId.ToString();
                        elo.EventDescription = el.EventDescription.Replace("\u0027", "'");
                        elo.EventName = el.EventName.Replace("\u0027", "'");
                        elo.ChefProfileURL = ServerMethods.ServerUrl + el.GrubClubUrlFriendlyName + "/" + el.SupperClubId.ToString();
                        eventListOutput.Add(elo);
                    }
                    LogMessage("Feed Event Listing: Results returned successfully.", LogLevel.DEBUG);
                    ViewData.Model = eventListOutput;
                    return View();
                }
            }
            catch (Exception ex)
            {
                LogMessage("Feed Event Listing: No result returned. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                ViewData.Model = "Error occurred while searching for event.";
                return View();                
            }
        }

        
    }    


}

