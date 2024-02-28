﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using SupperClub.Models;
using SupperClub.Domain.Repository;
using SupperClub.Domain;
using SupperClub.Code;
using System.Diagnostics;
using System.Net;
using System.IO;
using Facebook;
using System.Text;
using System.Web.Configuration;
using SupperClub.WebUI;
using SupperClub.Web.Helpers;
using System.Web.Script.Serialization;

namespace SupperClub.Controllers
{
    public class AccountController : BaseController
    {
        private string facebook_graph_host = WebConfigurationManager.AppSettings["Facebook_Graph_Host"];
        private string facebook_client_id = WebConfigurationManager.AppSettings["Facebook_API_Key"];
        private string facebook_client_secret = WebConfigurationManager.AppSettings["Facebook_API_Secret"];
        private string facebook_scheme = WebConfigurationManager.AppSettings["Facebook_Scheme"];
        private string facebook_path = WebConfigurationManager.AppSettings["Facebook_Path"];
        private string facebook_redirect_uri = WebConfigurationManager.AppSettings["Facebook_Redirect_Uri"];

        public AccountController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        #region Logging on and Registering

        #region Log On / Off

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult LogOn(string returnUrl)
        {
            //FormsAuthentication.SignOut();
            //Session.Abandon();
            //UserMethods.ClearAllSettingsInSession();
            //LogOnModel objLogOnModel = new LogOnModel();
            //if (Request.UrlReferrer != null)
            //{
            //    Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
            //    objLogOnModel.redirectUrl = uriAddress.PathAndQuery;
            //}
            //return View(objLogOnModel);
            LogMessage("New Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            if (!string.IsNullOrEmpty(returnUrl))
                lrm.RedirectURL = returnUrl;
            else if (Request.UrlReferrer != null)
            {
                Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
                lrm.RedirectURL = uriAddress.PathAndQuery;
            }
            lrm.LogOnModel = new LogOnModel();
            lrm.RegisterModel = new RegisterModel();
            lrm.ForgotPasswordModel = new ForgotPasswordModel();
            return View("LoginRegisterPage", lrm);
        }

        [HttpPost]
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (returnUrl == null || returnUrl == string.Empty)
                returnUrl = model.redirectUrl;

            if (ModelState.IsValid)
            {
                string username = model.UserName;
                // If the user has tried to log in using their email address grab the user name
                if (Membership.FindUsersByEmail(model.UserName).Count == 1)
                {
                    IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(model.UserName).Cast<MembershipUser>();
                    username = members.First().UserName;
                }

                if (Membership.FindUsersByName(username).Count != 1)
                {
                    SetNotification(NotificationType.Error, "Sorry we couldn't find your user name. You might need to register!", false, false, true);
                    return View(model);
                }

                // If we get this far the username definitely exists
                if (UserMethods.IsSpecificUserLockedOut(username))
                {
                    SetNotification(NotificationType.Error, "Your user account has been locked, please contact an Administrator!", false, false, true);
                    return View(model);
                }

                if (Membership.ValidateUser(model.UserName, model.Password))
                {
                    Session.Clear();
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    LogLoginDetails("Logged In: " + model.UserName, Request);
                    if (UserMethods.IsLoggedIn && UserMethods.CurrentUser.SegmentUser == null)
                    {
                        // Create a temp UserId
                        Guid tempUserId = Guid.NewGuid();
                        // check to see if this userid already exists
                        while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                        {
                            tempUserId = Guid.NewGuid();
                        }
                        SegmentUser su = new SegmentUser();
                        su.SegmentUserId = tempUserId;
                        su.UserId = UserMethods.CurrentUser.Id;
                        // Store the UserId
                        su = _supperClubRepository.AddSegmentUser(su);
                        if (su != null)
                        {
                            LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                            UserMethods.CurrentUser.SegmentUser = su;
                        }
                    }
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(HttpUtility.UrlDecode(returnUrl));
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    SetNotification(NotificationType.Error, "The password provided is incorrect.", false, false, true);
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            SetNotification(NotificationType.Error, "Login was unsuccessful. Please correct the errors and try again.", false, false, true);
            return View(model);
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult LogOnHost(string returnUrl)
        {
            //FormsAuthentication.SignOut();
            //Session.Abandon();
            //UserMethods.ClearAllSettingsInSession();
            //LogOnModel objLogOnModel = new LogOnModel();
            //if (Request.UrlReferrer != null)
            //{
            //    Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
            //    objLogOnModel.redirectUrl = uriAddress.PathAndQuery;
            //}
            //return View(objLogOnModel);
            LogMessage("New Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            if (!string.IsNullOrEmpty(returnUrl))
                lrm.RedirectURL = returnUrl;
            else if (Request.UrlReferrer != null)
            {
                Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
                lrm.RedirectURL = uriAddress.PathAndQuery;
            }
            lrm.LogOnModel = new LogOnModel();
            lrm.RegisterModel = new RegisterModel();
            lrm.ForgotPasswordModel = new ForgotPasswordModel();
            return View("LogOnHost", lrm);
        }

        public ActionResult LogOff(string returnUrl = "")
        {
            LogMessage("Logged Out");
            FormsAuthentication.SignOut();
            Session.Abandon();
            UserMethods.ClearAllSettingsInSession();

            // clear authentication cookie
            HttpCookie cookie1 = new HttpCookie(FormsAuthentication.FormsCookieName, "");
            cookie1.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie1);

            // clear session cookie
            HttpCookie cookie2 = new HttpCookie("ASP.NET_SessionId", "");
            cookie2.Expires = DateTime.Now.AddYears(-1);
            Response.Cookies.Add(cookie2);

            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            {
                return Redirect(HttpUtility.UrlDecode(returnUrl));
            }
            else if(!string.IsNullOrEmpty(returnUrl))
                return Redirect(HttpUtility.UrlDecode(returnUrl));
            else
                return RedirectToAction("Index", "Home");
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult Login(string returnUrl)
        {
            LogMessage("New Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            if (!string.IsNullOrEmpty(returnUrl))
                lrm.RedirectURL = returnUrl;
            else if (Request.UrlReferrer != null)
            {
                Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
                lrm.RedirectURL = uriAddress.PathAndQuery;
            }
            lrm.LogOnModel = new LogOnModel();
            lrm.RegisterModel = new RegisterModel();
            lrm.ForgotPasswordModel = new ForgotPasswordModel();
            return View("LoginRegisterPage", lrm);
        }
        [HttpPost]
        public ActionResult LogIn(EventViewModel model, string returnUrl)
        {

            if (ModelState.IsValidField("UserName") && ModelState.IsValidField("Password"))
            {
                string username = model.LogOnModel.UserName;
                // If the user has tried to log in using their email address grab the user name
                if (Membership.FindUsersByEmail(model.LogOnModel.UserName).Count == 1)
                {
                    IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(model.LogOnModel.UserName).Cast<MembershipUser>();
                    username = members.First().UserName;
                }

                if (Membership.FindUsersByName(username).Count != 1)
                {
                    SetNotification(NotificationType.Error, "Sorry we couldn't find your user name. You might need to register!", false, false, true);
                    return View(model);
                }

                // If we get this far the username definitely exists
                if (UserMethods.IsSpecificUserLockedOut(username))
                {
                    SetNotification(NotificationType.Error, "Your user account has been locked, please contact an Administrator!", false, false, true);
                    return View(model);
                }

                if (Membership.ValidateUser(model.LogOnModel.UserName, model.LogOnModel.Password))
                {
                    Session.Clear();
                    FormsAuthentication.SetAuthCookie(model.LogOnModel.UserName, model.LogOnModel.RememberMe);
                    LogLoginDetails("Logged In: " + model.LogOnModel.UserName, Request);
                    if(UserMethods.IsLoggedIn && UserMethods.CurrentUser.SegmentUser == null)
                    {
                        // Create a temp UserId
                        Guid tempUserId = Guid.NewGuid();
                        // check to see if this userid already exists
                        while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                        {
                            tempUserId = Guid.NewGuid();
                        }
                        SegmentUser su = new SegmentUser();
                        su.SegmentUserId = tempUserId;
                        su.UserId = UserMethods.CurrentUser.Id;
                        // Store the UserId
                        su = _supperClubRepository.AddSegmentUser(su);
                        if(su != null)
                        {
                            LogMessage("Successfully created segment user",Logger.LogLevel.INFO);
                            UserMethods.CurrentUser.SegmentUser = su;
                        }
                    }
                    
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(HttpUtility.UrlDecode(returnUrl));
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    SetNotification(NotificationType.Error, "The password provided is incorrect.", false, false, true);
                    return View(model);
                }
            }

            // If we got this far, something failed, redisplay form
            SetNotification(NotificationType.Error, "Login was unsuccessful. Please correct the errors and try again.", false, false, true);
            return View("LoginRegister");
        }

        [HttpPost]
        public ActionResult NewLogIn(LoginRegisterModel model, string returnUrl, bool isPage = false)
        {
            model.RedirectURL = returnUrl;
            model.RegisterModel = new RegisterModel();
            model.ForgotPasswordModel = new ForgotPasswordModel();
            //if (ModelState.IsValidField("UserName") && ModelState.IsValidField("Password"))
            if (ModelState.IsValid)
            {
                string username = model.LogOnModel.UserName;
                // If the user has tried to log in using their email address grab the user name
                if (Membership.FindUsersByEmail(model.LogOnModel.UserName).Count == 1)
                {
                    IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(model.LogOnModel.UserName).Cast<MembershipUser>();
                    username = members.First().UserName;
                }

                if (Membership.FindUsersByName(username).Count != 1)
                {
                    SetNotification(NotificationType.Error, "Sorry we couldn't find your user name. You might need to register!", false, false, true);
                    return View("LoginRegisterPage", model);
                }

                // If we get this far the username definitely exists
                if (UserMethods.IsSpecificUserLockedOut(username))
                {
                    SetNotification(NotificationType.Error, "Your user account has been locked, please contact an Administrator!", false, false, true);
                    return View("LoginRegisterPage", model);
                }

                if (Membership.ValidateUser(model.LogOnModel.UserName, model.LogOnModel.Password))
                {
                    Session.Clear();
                    FormsAuthentication.SetAuthCookie(model.LogOnModel.UserName, model.LogOnModel.RememberMe);
                    LogLoginDetails("Logged In: " + model.LogOnModel.UserName, Request);
                    if (UserMethods.IsLoggedIn && UserMethods.CurrentUser.SegmentUser == null)
                    {
                        // Create a temp UserId
                        Guid tempUserId = Guid.NewGuid();
                        // check to see if this userid already exists
                        while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                        {
                            tempUserId = Guid.NewGuid();
                        }
                        SegmentUser su = new SegmentUser();
                        su.SegmentUserId = tempUserId;
                        su.UserId = UserMethods.CurrentUser.Id;
                        // Store the UserId
                        su = _supperClubRepository.AddSegmentUser(su);
                        if (su != null)
                        {
                            LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                            UserMethods.CurrentUser.SegmentUser = su;
                            Segment.Analytics.Client.Identify(UserMethods.CurrentUser.SegmentUser.SegmentUserId.ToString(), new Segment.Model.Traits
                                {
                                    { "first_name", UserMethods.CurrentUser.FirstName },
                                    { "last_name", UserMethods.CurrentUser.LastName },
                                    { "email", UserMethods.CurrentUser.Email }
                                });
                            Segment.Analytics.Client.Track(UserMethods.CurrentUser.SegmentUser.SegmentUserId.ToString(), "Login");
                        }
                    }
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(HttpUtility.UrlDecode(returnUrl));
                    }
                    else if (!string.IsNullOrEmpty(returnUrl))
                    {
                        return Redirect(HttpUtility.UrlDecode(returnUrl));
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    SetNotification(NotificationType.Error, "The password provided is incorrect.", false, false, true);
                    return View("LoginRegisterPage", model);
                }
            }

            // If we got this far, something failed, redisplay form
            SetNotification(NotificationType.Error, "Login was unsuccessful. Please correct the errors and try again.", false, false, true);
            return View("LoginRegisterPage", model);
            
        }
        #endregion

        #region Facebook Login

        public ActionResult FacebookLogin(string returnUrl)
        {
            LogMessage("Started Facebook Login", Logger.LogLevel.DEBUG);
            string type = "type=web_server";
            string client_id = string.Format("client_id={0}", facebook_client_id);

            string fullurlredirect = string.Format("{0}://{1}/oauth/authorize?{2}&{3}&redirect_uri={4}{5}&state=/requested/page", facebook_scheme, facebook_graph_host, type, client_id, ServerMethods.ServerUrl.Replace("www.", ""), facebook_redirect_uri);

            if ((Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))||(!string.IsNullOrEmpty(returnUrl)))
            {
                TempData["RedirectURL"] = returnUrl;
            }

            LogMessage("Facebook Login Full URL Redirect: " + fullurlredirect, Logger.LogLevel.DEBUG);
            return new RedirectResult(fullurlredirect);
        }

        public ActionResult FacebookLoginOK(string code)
        {
            LogMessage("Facebook Returned Login OK. code=" + code + ", redirectUrl=" + TempData["RedirectURL"], Logger.LogLevel.DEBUG);
            //parameter code is the session token
            string redirectURL = string.Empty;
            User currentUser = null;
            if (!string.IsNullOrEmpty(code))
            {
                if (TempData["RedirectURL"] != null)
                    redirectURL = TempData["RedirectURL"].ToString();
                TempData["RedirectURL"] = redirectURL;

                string accessToken = Utils.GetAccessTokenFromFb(code);
                if (string.IsNullOrEmpty(accessToken))
                {
                    SetNotification(NotificationType.Error, "There was a problem with Facebook logon. Please try again.", false, false, true);
                    return RedirectToAction("Logon", "Account");
                }
                //Instantiate a new facebookClient from the C# SDK with the accessToken as parameter.
                var client = new FacebookClient(accessToken);

                //Uses a dynamic variable to handle the JSON result
                dynamic me = client.Get("me");

                //check to see if the user exists already
                currentUser = _supperClubRepository.GetUserByFbId(me.id);
                if (currentUser == null)
                {
                    // check for email if it doesn't exists ask for user's email otherwise register the user
                    if (!string.IsNullOrEmpty(me.email))
                    {
                        MembershipCreateStatus createStatus;
                        string newPassword = PasswordGenerator.GeneratePassword();
                        MembershipUser membershipUser = Membership.CreateUser(me.email, newPassword, me.email, null, null, true, null, out createStatus);
                        bool success = AddFBUser(membershipUser, me);

                        if (createStatus == MembershipCreateStatus.Success && success)
                        {
                            if (Membership.ValidateUser(me.email, newPassword))
                            {
                                EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                                em.SendWelcomeEmail(me.first_name, membershipUser.Email);
                                FormsAuthentication.SetAuthCookie(me.username, false /* createPersistentCookie */);
                                HttpCookie cookie = new HttpCookie("SetTag", "1");
                                HttpContext.Response.Cookies.Remove("SetTag");
                                HttpContext.Response.SetCookie(cookie);
                                LogLoginDetails("Logged In via FB (new user): " + membershipUser.Email, Request);
                                // Create a temp UserId
                                Guid tempUserId = Guid.NewGuid();
                                // check to see if this userid already exists
                                while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                                {
                                    tempUserId = Guid.NewGuid();
                                }
                                SegmentUser su = new SegmentUser();
                                su.SegmentUserId = tempUserId;
                                su.UserId = UserMethods.CurrentUser.Id;
                                // Store the UserId
                                su = _supperClubRepository.AddSegmentUser(su);
                                if (su != null)
                                {
                                    LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                    UserMethods.CurrentUser.SegmentUser = su;
                                    Segment.Analytics.Client.Identify(currentUser.SegmentUser.SegmentUserId.ToString(), new Segment.Model.Traits
                                    {
                                        { "first_name", currentUser.FirstName },
                                        { "last_name", currentUser.LastName },
                                        { "email", currentUser.Email }
                                    });
                                    Segment.Analytics.Client.Track(currentUser.SegmentUser.SegmentUserId.ToString(), "FB Registration");
                                }
                                if (!string.IsNullOrEmpty(redirectURL))
                                    return Redirect(redirectURL);
                                return RedirectToAction("Index", "Home");
                            }
                        }
                    }
                    else
                    {
                        // if email is not provided for a user coming from FB force them to add their email address, pass access token string from FB to the page as well via Facebook user model
                        FacebookUser fbUser = new FacebookUser();
                        fbUser.Email = me.email;
                        fbUser.FbAcessToken = accessToken;
                        return View("FBUserDetails", fbUser);
                    }
                }
                // if user already exists but have not registered their details through the site update their details based on FB info
                else if (currentUser.FBUserOnly)
                {
                    LogMessage("FacebookLoginOK in currentUser.FBUserOnly block", Logger.LogLevel.DEBUG);
                    
                    ConvertFBUserToDomainUser(currentUser, me);
                    LogMessage("FacebookLoginOKc currentUser.FBUserOnly block -  Converted fb user to domain user", Logger.LogLevel.DEBUG);
                    
                    bool _updateSuccess = _supperClubRepository.UpdateUser(currentUser);
                    if (_updateSuccess)
                        LogMessage("FacebookLoginOKc currentUser.FBUserOnly block -  Updated user. userid=" + currentUser.Id.ToString(), Logger.LogLevel.DEBUG);
                    else
                        LogMessage("FacebookLoginOKc currentUser.FBUserOnly block -  Error updating user. userid=" + currentUser.Id.ToString(), Logger.LogLevel.ERROR);

                    User _user = _supperClubRepository.GetUser(currentUser.Id);
                    if (_user != null && _user.Id != null)
                    {
                        LogMessage("FacebookLoginOK got current user details after updating them. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                        System.Web.HttpContext.Current.Session["User"] = _user;
                        currentUser = _user;
                        if (currentUser.SegmentUser == null)
                        {
                            // Create a temp UserId
                            Guid tempUserId = Guid.NewGuid();
                            // check to see if this userid already exists
                            while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                            {
                                tempUserId = Guid.NewGuid();
                            }
                            SegmentUser su = new SegmentUser();
                            su.SegmentUserId = tempUserId;
                            su.UserId = _user.Id;
                            // Store the UserId
                            su = _supperClubRepository.AddSegmentUser(su);
                            if (su != null)
                            {
                                LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                currentUser.SegmentUser = su;
                            
                                Segment.Analytics.Client.Identify(currentUser.SegmentUser.SegmentUserId.ToString(), new Segment.Model.Traits
                                {
                                    { "first_name", currentUser.FirstName },
                                    { "last_name", currentUser.LastName },
                                    { "email", currentUser.Email }
                                });
                                Segment.Analytics.Client.Track(currentUser.SegmentUser.SegmentUserId.ToString(), "FB Login");
                            }
                        }
                        LogMessage("FacebookLoginOK successfully assigned the updated user to current user. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                        FormsAuthentication.SetAuthCookie(_user.Email, false);
                        LogMessage("FacebookLoginOK successfully set cookie for login. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                        
                    }
                }
                else if (!currentUser.FBUserOnly)
                {
                    LogMessage("FacebookLoginOK in !currentUser.FBUserOnly block", Logger.LogLevel.DEBUG);
                    try
                    {
                        currentUser.FBUserOnly = false;
                        currentUser.FBJson = me.ToString();
                        LogMessage("FacebookLoginOK in !currentUser.FBUserOnly block sending details to DB for update", Logger.LogLevel.DEBUG);
                        bool _updateSuccess = _supperClubRepository.UpdateUser(currentUser);
                        if (_updateSuccess)
                            LogMessage("FacebookLoginOKc currentUser.FBUserOnly block -  Updated updated current user details to DB successfully. userid=" + currentUser.Id.ToString(), Logger.LogLevel.DEBUG);
                        else
                            LogMessage("FacebookLoginOKc currentUser.FBUserOnly block -  Error updating user. userid=" + currentUser.Id.ToString(), Logger.LogLevel.ERROR);

                        User _user = _supperClubRepository.GetUser(currentUser.Id);
                        LogMessage("FacebookLoginOK in !currentUser.FBUserOnly block got user details", Logger.LogLevel.DEBUG);
                        if (_user != null && _user.Id != null)
                        {
                            LogMessage("FacebookLoginOK !currentUser.FBUserOnly block got current user details after updating them. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                            System.Web.HttpContext.Current.Session["User"] = _user;
                            currentUser = _user;
                            if(currentUser.SegmentUser == null)
                            {
                                // Create a temp UserId
                                Guid tempUserId = Guid.NewGuid();
                                // check to see if this userid already exists
                                while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                                {
                                    tempUserId = Guid.NewGuid();
                                }
                                SegmentUser su = new SegmentUser();
                                su.SegmentUserId = tempUserId;
                                su.UserId = _user.Id;
                                // Store the UserId
                                su = _supperClubRepository.AddSegmentUser(su);
                                if (su != null)
                                {
                                    LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                    currentUser.SegmentUser = su;
                                    Segment.Analytics.Client.Identify(currentUser.SegmentUser.SegmentUserId.ToString(), new Segment.Model.Traits
                                    {
                                        { "first_name", currentUser.FirstName },
                                        { "last_name", currentUser.LastName },
                                        { "email", currentUser.Email }
                                    });
                                    Segment.Analytics.Client.Track(currentUser.SegmentUser.SegmentUserId.ToString(), "FB Login");
                                }
                            }
                            LogMessage("FacebookLoginOK !currentUser.FBUserOnly block successfully assigned the updated user to current user. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                            FormsAuthentication.SetAuthCookie(_user.Email, false);
                            LogMessage("FacebookLoginOK !currentUser.FBUserOnly block successfully set cookie for login. UserId=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);                        
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage("FacebookLoginOK in !currentUser.FBUserOnly block ERROR: Exception Details: " + ex.Message + "  Inner Exception: " + (ex.InnerException != null ? ex.InnerException.StackTrace : "No Inner Exception.") + "  Stack Trace Info: " + ex.StackTrace + "  User Agent: " + Request.UserAgent, Logger.LogLevel.ERROR);
                    }
                }
            }
            if (currentUser != null && currentUser.Email != null)
                LogLoginDetails("Logged In via FB: " + currentUser.Email, Request);
            if (!string.IsNullOrEmpty(redirectURL))
                return Redirect(redirectURL);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult FacebookUserAddEmail(FacebookUser fbUser)
        {
            LogMessage("Started Facebook New User Adding Email", Logger.LogLevel.DEBUG);
            string redirectURL = string.Empty;
            if (TempData["RedirectURL"] != null)
                redirectURL = TempData["RedirectURL"].ToString();
            TempData["RedirectURL"] = redirectURL;

            if (ModelState.IsValid)
            {
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(fbUser.Email);
                    validEmail = true;
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    ModelState.AddModelError("Email", "Invalid E-mail Address");
                    SetNotification(NotificationType.Error, "Registration was unsuccessful. E-mail address entered is invalid, please check the e-mail address and try again.", false, false, true);
                }
                else
                {
                    //Uses a dynamic variable to handle the JSON result
                    var client = new FacebookClient(fbUser.FbAcessToken);

                    //Uses a dynamic variable to handle the JSON result
                    dynamic me = client.Get("me");
                    string newPassword = PasswordGenerator.GeneratePassword();

                    MembershipCreateStatus createStatus;
                    MembershipUser membershipUser = Membership.CreateUser(fbUser.Email, newPassword, fbUser.Email, null, null, true, null, out createStatus);

                    me.email = fbUser.Email;
                    bool success = AddFBUser(membershipUser, me);

                    if (createStatus == MembershipCreateStatus.Success && success)
                    {
                        if (Membership.ValidateUser(fbUser.Email, newPassword))
                        {
                            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                            em.SendWelcomeEmail(me.first_name, membershipUser.Email);
                            FormsAuthentication.SetAuthCookie(fbUser.Email, false /* createPersistentCookie */);
                            HttpCookie cookie = new HttpCookie("SetTag", "1");
                            HttpContext.Response.Cookies.Remove("SetTag");
                            HttpContext.Response.SetCookie(cookie);
                            LogLoginDetails("Logged In via FB (new user - provided email): " + membershipUser.Email, Request);
                            if (!string.IsNullOrEmpty(redirectURL))
                                return Redirect(redirectURL);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Email", ErrorCodeToString(createStatus));
                        SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
                    }
                }
            }
            return View("FBUserDetails", fbUser);
        }

        #endregion

        #region Register

        
        public ActionResult Register(string returnUrl)
        {
            LogMessage("New User Begin Registration");
            //var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            //ViewBag.Gender = new SelectList(genders);

            if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
            {
                TempData["RedirectURL"] = returnUrl;
            }

            return View();
        }

        [HttpPost]        
        public ActionResult Register(RegisterModel model)
        {
            string redirectURL = string.Empty;
            if (TempData["RedirectURL"] != null)
                redirectURL = TempData["RedirectURL"].ToString();
            TempData["RedirectURL"] = redirectURL;

            if (ModelState.IsValid)
            {
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(model.Email);
                    validEmail = true;
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    ModelState.AddModelError("", "Invalid E-mail Address");
                    SetNotification(NotificationType.Error, "Registration was unsuccessful. E-mail address entered is invalid, please check the e-mail address and try again.", false, false, true);
                }
                else
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    MembershipUser membershipUser = Membership.CreateUser(model.Email, model.Password, model.Email, null, null, true, null, out createStatus);
                    bool success = AddUser(membershipUser, model);

                    if (createStatus == MembershipCreateStatus.Success && success)
                    {
                        if (Membership.ValidateUser(model.Email, model.Password))
                        {
                            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                            em.SendWelcomeEmail(model.FirstName, membershipUser.Email);
                            FormsAuthentication.SetAuthCookie(model.Email, false /* createPersistentCookie */);
                            LogLoginDetails("Registration complete. Logged In: " + model.Email, Request);
                            HttpCookie cookie = new HttpCookie("SetTag", "1");
                            HttpContext.Response.Cookies.Remove("SetTag");
                            HttpContext.Response.SetCookie(cookie);
                            // Create a temp UserId
                            Guid tempUserId = Guid.NewGuid();
                            // check to see if this userid already exists
                            while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                            {
                                tempUserId = Guid.NewGuid();
                            }
                            SegmentUser su = new SegmentUser();
                            su.SegmentUserId = tempUserId;
                            su.UserId = UserMethods.CurrentUser.Id;
                            // Store the UserId
                            su = _supperClubRepository.AddSegmentUser(su);
                            if (su != null)
                            {
                                LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                UserMethods.CurrentUser.SegmentUser = su;
                            }
                            if (!string.IsNullOrEmpty(redirectURL))
                                return Redirect(redirectURL);
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                        SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            //var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            //ViewBag.Gender = new SelectList(genders);
            return View(model);
        }

        [HttpPost]        
        public ActionResult RegisterNew(EventViewModel model, string redirectURL)
        {

            if (ModelState.IsValidField("FirstName") && ModelState.IsValidField("LastName") && ModelState.IsValidField("Email") && ModelState.IsValidField("Password"))
            {
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(model.RegisterModel.Email);
                    validEmail = true;
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    ModelState.AddModelError("", "Invalid E-mail Address");
                    SetNotification(NotificationType.Error, "Registration was unsuccessful. E-mail address entered is invalid, please check the e-mail address and try again.", false, false, true);
                }
                else
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    MembershipUser membershipUser = Membership.CreateUser(model.RegisterModel.Email, model.RegisterModel.Password, model.RegisterModel.Email, null, null, true, null, out createStatus);
                    bool success = AddUser(membershipUser, model.RegisterModel);

                    if (createStatus == MembershipCreateStatus.Success && success)
                    {
                        if (Membership.ValidateUser(model.RegisterModel.Email, model.RegisterModel.Password))
                        {
                            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                            em.SendWelcomeEmail(model.RegisterModel.FirstName, membershipUser.Email);
                            FormsAuthentication.SetAuthCookie(model.RegisterModel.Email, false /* createPersistentCookie */);
                            LogLoginDetails("Registration complete. Logged In: " + model.RegisterModel.Email, Request);
                            // Create a temp UserId
                            Guid tempUserId = Guid.NewGuid();
                            // check to see if this userid already exists
                            while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                            {
                                tempUserId = Guid.NewGuid();
                            }
                            SegmentUser su = new SegmentUser();
                            su.SegmentUserId = tempUserId;
                            su.UserId = UserMethods.CurrentUser.Id;
                            // Store the UserId
                            su = _supperClubRepository.AddSegmentUser(su);
                            if (su != null)
                            {
                                LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                UserMethods.CurrentUser.SegmentUser = su;
                            }
                            if (!string.IsNullOrEmpty(redirectURL))
                            {
                                if (Url.IsLocalUrl(redirectURL) && redirectURL.Length > 1 && redirectURL.StartsWith("/")
                                    && !redirectURL.StartsWith("//") && !redirectURL.StartsWith("/\\"))
                                {
                                    return Redirect(HttpUtility.UrlDecode(redirectURL));
                                }
                                else
                                {
                                    return Redirect(redirectURL);
                                }
                            }
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                        SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            //var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            //ViewBag.Gender = new SelectList(genders);
            return View("LoginRegister");
        }
        [HttpPost]        
        public ActionResult NewRegister(LoginRegisterModel model, string redirectURL, bool isPage=false)
        {
            model.RedirectURL = redirectURL;
            model.LogOnModel = new LogOnModel();
            model.ForgotPasswordModel = new ForgotPasswordModel();
            if (ModelState.IsValid)
            {
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(model.RegisterModel.Email);
                    validEmail = true;
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    ModelState.AddModelError("", "Invalid E-mail Address");
                    SetNotification(NotificationType.Error, "Registration was unsuccessful. E-mail address entered is invalid, please check the e-mail address and try again.", false, false, true);
                    return View("LoginRegisterPage", model);
                }
                else
                {
                    // Attempt to register the user
                    MembershipCreateStatus createStatus;
                    MembershipUser membershipUser = Membership.CreateUser(model.RegisterModel.Email, model.RegisterModel.Password, model.RegisterModel.Email, null, null, true, null, out createStatus);
                    bool success = AddUser(membershipUser, model.RegisterModel);

                    if (createStatus == MembershipCreateStatus.Success && success)
                    {
                        if (Membership.ValidateUser(model.RegisterModel.Email, model.RegisterModel.Password))
                        {
                            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                            em.SendWelcomeEmail(model.RegisterModel.FirstName, membershipUser.Email);
                            FormsAuthentication.SetAuthCookie(model.RegisterModel.Email, false /* createPersistentCookie */);
                            LogLoginDetails("Registration complete. Logged In: " + model.RegisterModel.Email, Request);
                            HttpCookie cookie = new HttpCookie("SetTag","1");
                            HttpContext.Response.Cookies.Remove("SetTag");
                            HttpContext.Response.SetCookie(cookie);
                            
                            // Create a temp UserId
                            Guid tempUserId = Guid.NewGuid();
                            // check to see if this userid already exists
                            while (_supperClubRepository.IsExistSegmentUserId(tempUserId) != false)
                            {
                                tempUserId = Guid.NewGuid();
                            }
                            SegmentUser su = new SegmentUser();
                            su.SegmentUserId = tempUserId;
                            su.UserId = (Guid)membershipUser.ProviderUserKey;
                            // Store the UserId
                            su = _supperClubRepository.AddSegmentUser(su);
                            if (su != null)
                            {
                                LogMessage("Successfully created segment user", Logger.LogLevel.INFO);
                                Segment.Analytics.Client.Identify(su.SegmentUserId.ToString(), new Segment.Model.Traits
                                {
                                    { "first_name", model.RegisterModel.FirstName },
                                    { "last_name", model.RegisterModel.LastName },
                                    { "email", model.RegisterModel.Email }
                                });
                                Segment.Analytics.Client.Track(su.SegmentUserId.ToString(), "New User Registration");                        
                            }
                            
                            if (!string.IsNullOrEmpty(redirectURL))
                            {
                                if (Url.IsLocalUrl(redirectURL) && redirectURL.Length > 1 && redirectURL.StartsWith("/")
                                    && !redirectURL.StartsWith("//") && !redirectURL.StartsWith("/\\"))
                                {
                                    return Redirect(HttpUtility.UrlDecode(redirectURL));
                                }
                                else
                                {
                                    return Redirect(HttpUtility.UrlDecode(redirectURL));
                                }
                            }
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", ErrorCodeToString(createStatus));
                        SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
                        return View("LoginRegisterPage", model);
                    }
                }
            }
            // If we got this far, something failed, redisplay form
            //var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            //ViewBag.Gender = new SelectList(genders);
            // If we got this far, something failed, redisplay form
            SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
            return View("LoginRegisterPage", model);
        }
        #endregion

        #region New Login Register methods
        public ActionResult LoginRegister(string returnUrl)
        {
            LogMessage("New Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            if(!string.IsNullOrEmpty(returnUrl))
                lrm.RedirectURL = returnUrl;
            lrm.LogOnModel = new LogOnModel();
            lrm.RegisterModel = new RegisterModel();
            lrm.ForgotPasswordModel = new ForgotPasswordModel();
            return View("LoginRegister", lrm);
        }

        [HttpPost]
        public ActionResult LoginRegister(LoginRegisterModel model)
        {
            LogMessage("New Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            //if(!string.IsNullOrEmpty(returnUrl))
            //    lrm.RedirectURL = returnUrl;
            return View("LoginRegister", lrm);

            //if (ModelState.IsValid)
            //{
                //#region Registration Button Click
                //if (model.Register)
                //{
                //    bool validEmail = false;
                //    // Validate the email first
                //    try
                //    {
                //        var addr = new System.Net.Mail.MailAddress(model.RegisterModel.Email);
                //        validEmail = true;
                //    }
                //    catch
                //    {
                //        validEmail = false;
                //    }
                //    if (!validEmail)
                //    {
                //        ModelState.AddModelError("", "Invalid E-mail Address");
                //        SetNotification(NotificationType.Error, "Registration was unsuccessful. E-mail address entered is invalid, please check the e-mail address and try again.", false, false, true);
                //    }
                //    else
                //    {
                //        // Attempt to register the user
                //        MembershipCreateStatus createStatus;
                //        MembershipUser membershipUser = Membership.CreateUser(model.RegisterModel.Email, model.RegisterModel.Password, model.RegisterModel.Email, null, null, true, null, out createStatus);
                //        bool success = AddUser(membershipUser, model.RegisterModel);

                //        if (createStatus == MembershipCreateStatus.Success && success)
                //        {
                //            if (Membership.ValidateUser(model.RegisterModel.Email, model.RegisterModel.Password))
                //            {
                //                EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                //                em.SendWelcomeEmail(model.RegisterModel.FirstName, membershipUser.Email);
                //                FormsAuthentication.SetAuthCookie(model.RegisterModel.Email, false /* createPersistentCookie */);
                //                LogLoginDetails("Registration complete. Logged In: " + model.RegisterModel.Email, Request);
                //                if (!string.IsNullOrEmpty(model.RedirectURL))
                //                    return Redirect(model.RedirectURL);
                //                if (!string.IsNullOrEmpty(model.ActionName) && !string.IsNullOrEmpty(model.ControllerName))
                //                {
                //                    if (!string.IsNullOrEmpty(model.Parameters))
                //                    {
                //                        JavaScriptSerializer jss = new JavaScriptSerializer();
                //                        Dictionary<string, object> dparam = new Dictionary<string, object>();
                //                        Dictionary<string, object> obj = jss.Deserialize<Dictionary<string, object>>(model.Parameters);
                //                        string parameterList = "";
                //                        int cnt = 1;
                //                        foreach (KeyValuePair<string, object> o in obj)
                //                        {
                //                            parameterList += o.Key + "=" + o.Value;
                //                            if (obj.Count <= cnt)
                //                                parameterList += ",";
                //                            cnt++;
                //                        }
                //                        return RedirectToAction(model.ActionName, model.ControllerName, new { parameterList });
                //                    }
                //                    else
                //                        return RedirectToAction(model.ActionName, model.ControllerName);
                //                }
                //                return RedirectToAction("Index", "Home");
                //            }
                //        }
                //        else
                //        {
                //            ModelState.AddModelError("", ErrorCodeToString(createStatus));
                //            SetNotification(NotificationType.Error, "Registration was unsuccessful. Please correct the errors and try again.", false, false, true);
                //        }
                //    }
                //}
                //#endregion

                //#region Login Button Click
                //if (model.Login)
                //{
                //    string username = model.LogOnModel.UserName;
                //    // If the user has tried to log in using their email address grab the user name
                //    if (Membership.FindUsersByEmail(model.LogOnModel.UserName).Count == 1)
                //    {
                //        IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(model.LogOnModel.UserName).Cast<MembershipUser>();
                //        username = members.First().UserName;
                //    }

                //    if (Membership.FindUsersByName(username).Count != 1)
                //    {
                //        SetNotification(NotificationType.Error, "Sorry we couldn't find your user name. You might need to register!", false, false, true);
                //        return View(model);
                //    }

                //    // If we get this far the username definitely exists
                //    if (UserMethods.IsSpecificUserLockedOut(username))
                //    {
                //        SetNotification(NotificationType.Error, "Your user account has been locked, please contact an Administrator!", false, false, true);
                //        return View(model);
                //    }

                //    if (Membership.ValidateUser(model.LogOnModel.UserName, model.LogOnModel.Password))
                //    {
                //        Session.Clear();
                //        FormsAuthentication.SetAuthCookie(model.LogOnModel.UserName, model.LogOnModel.RememberMe);
                //        LogLoginDetails("Logged In: " + model.LogOnModel.UserName, Request);

                //        if (!string.IsNullOrEmpty(model.RedirectURL))
                //            return Redirect(model.RedirectURL);
                //        if (!string.IsNullOrEmpty(model.ActionName) && !string.IsNullOrEmpty(model.ControllerName))
                //        {
                //            if (!string.IsNullOrEmpty(model.Parameters))
                //            {
                //                JavaScriptSerializer jss = new JavaScriptSerializer();
                //                Dictionary<string, object> dparam = new Dictionary<string, object>();
                //                Dictionary<string, object> obj = jss.Deserialize<Dictionary<string, object>>(model.Parameters);
                //                string parameterList = "";
                //                int cnt = 1;
                //                foreach (KeyValuePair<string, object> o in obj)
                //                {
                //                    parameterList += o.Key + "=" + o.Value;
                //                    if (obj.Count <= cnt)
                //                        parameterList += ",";
                //                    cnt++;
                //                }
                //                return RedirectToAction(model.ActionName, model.ControllerName, new { parameterList });
                //            }
                //            else
                //                return RedirectToAction(model.ActionName, model.ControllerName);
                //        }
                //    }
                //    else
                //    {
                //        SetNotification(NotificationType.Error, "The password provided is incorrect.", false, false, true);
                //        return View(model);
                //    }
                //}
                //#endregion
            //}
            //else
            //{
            //    var errors = ModelState.Select(x => x.Value.Errors)
            //                           .Where(y => y.Count > 0)
            //                           .ToList();
            //}
            //// If we got this far, something failed, redisplay form
            //return View(model);
        }
        #endregion

        #region User Login helper methods

        private bool AddUser(MembershipUser membershipUser, RegisterModel model)
        {
            try
            {
                if (membershipUser == null)
                    return false;

                User _user = new User();
                _user.Id = (Guid)membershipUser.ProviderUserKey;
                _user.FirstName = model.FirstName;
                _user.LastName = model.LastName;
                //_user.ContactNumber = model.ContactNumber;
                // _user.Gender = model.Gender.ToString();
                model.Role = "Guest";
                bool success = _supperClubRepository.CreateUser(_user);
                if (success)
                    AddToRole(membershipUser, model.Role);
                return success;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return false;
        }

        private bool AddFBUser(MembershipUser membershipUser, dynamic model)
        {
            try
            {
                if (membershipUser == null)
                    return false;

                User _user = new User();
                ConvertFBUserToDomainUser(_user, model);
                _user.Id = (Guid)membershipUser.ProviderUserKey;
                model.Role = "Guest";
                bool success = _supperClubRepository.CreateUser(_user);
                if (success)
                    AddToRole(membershipUser, model.Role);
                return success;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return false;
        }

        private void ConvertFBUserToDomainUser(User user, dynamic model)
        {
            try
            {
                user.FirstName = model.first_name;
                user.LastName = model.last_name;

                if (model.gender == "male")
                    user.Gender = Gender.Male.ToString();
                else if (model.gender == "female")
                    user.Gender = Gender.Female.ToString();
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

        private void LogLoginDetails(string message, HttpRequestBase request)
        {
            // Grab the device info
            StringBuilder deviceInfo = new StringBuilder();
            deviceInfo.AppendLine("DEVICE INFO");
            deviceInfo.AppendLine(string.Format("Mobile Device: {0}", request.Browser.IsMobileDevice));
            if (request.Browser.IsMobileDevice)
            {
                deviceInfo.AppendLine(string.Format("Mobile Manufacturer: {0}", request.Browser.MobileDeviceManufacturer));
                deviceInfo.AppendLine(string.Format("Mobile Model: {0}", request.Browser.MobileDeviceModel));
            }
            deviceInfo.AppendLine(string.Format("Screen Size: {0} x {1}", request.Browser.ScreenPixelsWidth, request.Browser.ScreenPixelsHeight));
            deviceInfo.AppendLine(string.Format("Platform: {0}", request.Browser.Platform));
            deviceInfo.AppendLine(string.Format("Browser: {0} v{1}", request.Browser.Browser, request.Browser.Version));

            LogLongMessage(message, deviceInfo.ToString());
        }

        #endregion

        #endregion

        #region Change / Reset Password

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                    LogMessage("Changed Password");
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    SetNotification(NotificationType.Success, "Your password has been updated.", false, true);
                    return View();
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    SetNotification(NotificationType.Error, "Password change was unsuccessful. Please correct the errors and try again.", false, false, true);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (Membership.FindUsersByEmail(model.EmailAddress).Count != 1)
                {
                    SetNotification(NotificationType.Error, "We don't have that email address the system, you sure it was the right one?", false, true, false);
                    return View(model);
                }

                MembershipUser aspnetuser = Membership.GetUser(model.EmailAddress);
                // Reset a new custom generated password
                string resetPassword = aspnetuser.ResetPassword();
                string newPassword = PasswordGenerator.GeneratePassword();
                try
                {
                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    aspnetuser.ChangePassword(resetPassword, newPassword);
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendPasswordResetEmail(aspnetuser.UserName, newPassword);
                    if (success)
                    {
                        LogMessage("Reset Password for: " + model.EmailAddress);
                        SetNotification(NotificationType.Success, "You have been sent an email with a new password, please check your SPAM folder just in case!", false, true, true);
                        return View();
                    }
                    else
                    {
                        SetNotification(NotificationType.Error, "Your password was reset but an email couldn't be sent. Please contact an Administrator.", false, true, true);
                        return View(model);
                    }

                }
                catch (Exception ex)
                {
                    log.Error(ex.Message, ex.InnerException);
                    SetNotification(NotificationType.Error, "Your password was unable to be reset. Please contact an Administrator.", true, false);
                    return View(model);
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult NewForgotPassword(string email)
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
                return Json(new { success = false, message = "E-mail address entered is invalid, please check the e-mail address and try again." }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (Membership.FindUsersByEmail(email).Count != 1)
                {
                    return Json(new { success = false, message = "We don't have that email address in the system, are you sure this is the right one?" }, JsonRequestBehavior.AllowGet);
                }
                
                try
                {
                    MembershipUser aspnetuser = Membership.GetUser(email);
                    // Reset a new custom generated password
                    string resetPassword = aspnetuser.ResetPassword();
                    string newPassword = PasswordGenerator.GeneratePassword();
                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    aspnetuser.ChangePassword(resetPassword, newPassword);
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendPasswordResetEmail(aspnetuser.UserName, newPassword);
                    if (success)
                    {
                        LogMessage("Reset Password for: " + email);
                        return Json(new { success = true, message = "You have been sent an email with a new password, please check your SPAM folder just in case!" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Your password was reset but an email couldn't be sent. Please contact an Administrator." }, JsonRequestBehavior.AllowGet);
                    }

                }
                catch (Exception ex)
                {
                    LogMessage("Error NewForgotPassword. Email=" + email + " ErrorMessage="+ ex.Message + " StackTrace=" + ex.StackTrace);
                    return Json(new { success = false, message = "Sorry, there was an error resetting your password. Please contact an Administrator." }, JsonRequestBehavior.AllowGet);
                    
                }
            }
        }
        #endregion

        #region User Details

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [Authorize]
        public ActionResult UserDetails()
        {
            User model = _supperClubRepository.GetUser(UserMethods.UserId);

            var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            // Convert to SelectListItems for binding
            List<string> Genders = new List<string>();
            foreach (Gender g in genders)
                Genders.Add(g.ToString());

            IEnumerable<SelectListItem> selectList = Genders.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (x == model.Gender)
            });

            SelectList sList = new SelectList(selectList, "Text", "Value", model.Gender);

            ViewBag.GenderList = sList;

            UserDetailModel _model = new UserDetailModel();
            _model.user = model;
            _model.changePasswordModel = new ChangePasswordModel();

            return View(_model);
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [Authorize]
        [HttpPost]
        public ActionResult UserDetails(UserDetailModel model)
        {
            //Post does an update
            var genders = Enum.GetValues(typeof(SupperClub.WebUI.Gender));
            // Convert to SelectListItems for binding
            List<string> Genders = new List<string>();
            foreach (Gender g in genders)
                Genders.Add(g.ToString());

            IEnumerable<SelectListItem> selectList = Genders.Select(x => new SelectListItem
            {
                Value = x,
                Text = x,
                Selected = (x == model.user.Gender)
            });

            SelectList sList = new SelectList(selectList, "Text", "Value", model.user.Gender);

            ViewBag.GenderList = sList;

            model.user.FBUserOnly = false;

            if (!string.IsNullOrEmpty(model.user.ContactNumber))
            {
                model.user.ContactNumber = model.user.ContactNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                model.user.ContactNumber = model.user.ContactNumber.Substring(model.user.ContactNumber.Length - 10);
                double number = 0;
                if (double.TryParse(model.user.ContactNumber, out number))
                    model.user.ContactNumber = number.ToString();
                else
                    model.user.ContactNumber = null;
            }

            bool changePasswordSucceeded;
            if (ModelState.IsValid)
            {
                try
                {
                    if (!string.IsNullOrEmpty(model.changePasswordModel.OldPassword) && !string.IsNullOrEmpty(model.changePasswordModel.NewPassword))
                    {
                        MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                        changePasswordSucceeded = currentUser.ChangePassword(model.changePasswordModel.OldPassword, model.changePasswordModel.NewPassword);
                        LogMessage("Changed Password");
                    }
                    else
                        changePasswordSucceeded = true;
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (_supperClubRepository.UpdateUser(model.user) && changePasswordSucceeded)
                    SetNotification(NotificationType.Info, "Your details were successfully updated", true);
                else
                {
                    SetNotification(NotificationType.Error, "There was an error updating your details", true);
                    if (!changePasswordSucceeded)
                    {

                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                        SetNotification(NotificationType.Error, "Password change was unsuccessful. Please correct the errors and try again.", false, false, true);
                    }
                }
            }
            else
            {
                LogMessage("Problems with User details update");
                string errorMsg = "There were problems updating your details, please fix them and try again.";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMsg = errorMsg + " " + error.ErrorMessage;
                    }
                }
                SetNotification(NotificationType.Error, errorMsg);
            }

            model.user = _supperClubRepository.GetUser(model.user.Id);
            System.Web.HttpContext.Current.Session["User"] = model.user;

            return View(model);
        }

        #endregion
        #region User Profile
        [Authorize]
        public ActionResult MyProfile(bool showGuest = false)
        {
            User model = _supperClubRepository.GetUser(UserMethods.UserId);
            if (model.FutureEvents.Count > 0)
                ViewBag.NextEvent = model.FutureEvents.OrderBy(a => a.Start).ToList<Event>()[0];

            if (model.FutureEvents.Count > 0)

                ViewBag.NextEvent = model.FutureEvents.OrderBy(a => a.Start).ToList<Event>()[0];


            if (UserMethods.CurrentUser.SupperClubs != null && UserMethods.CurrentUser.SupperClubs.Count > 0 && !showGuest)
            {
                SupperClub.Domain.SupperClub _supperclub = _supperClubRepository.GetSupperClubForUser(UserMethods.CurrentUser.Id);
                return RedirectToAction("ChefProfile", new { supperclubid = _supperclub.Id });
            }
            else
                return View(model);
        }
        [Authorize]
        public JsonResult LoadUserData(Guid userId)
        {
            User model = _supperClubRepository.GetUser(UserMethods.UserId);

            return Json(new { futureEvents = GetFutureEvents(model), pastEvents = GetPastEvents(model), reviews = GetReviews(model), wishListedEvents = GetWishListedEvents(model), followedSupperClubs = GetFollowedSupperClubs(model) },JsonRequestBehavior.AllowGet);
       
        }

        public JsonResult LoadDefaultData(int supperClubId)
        {

            var model = _supperClubRepository.GetSupperClub(supperClubId);



            return Json(new { futureEvents = GetFutureEvents(model), pastEvents = GetPastEvents(model), privateEvents = GetPrivateEvents(model), cancelEvents = GetRejectedAndCancelEvents(model), newEvents = GetNewEvents(model), reviews = GetReviews(model), followers = GetSupperClubFollowers(model) });

        }

        [Authorize(Roles="Host,Admin")]
        public ActionResult ChefProfile(int supperClubId)
        {

            if (supperClubId == UserMethods.SupperClubId || UserMethods.IsAdmin)
            {
                int Id;
                if (UserMethods.IsAdmin)
                    Id = supperClubId;
                else
                {
                    Domain.SupperClub _supperclub = _supperClubRepository.GetSupperClubForUser(UserMethods.CurrentUser.Id);
                    Id = _supperclub.Id;
                }
                SupperClubModel vmodel = new SupperClubModel();
                var model = _supperClubRepository.GetSupperClub(Id);


                ViewBag.FollowersCount = _supperClubRepository.GetSupperClubFollowers(model.Id);

                if (model.FutureEvents != null && model.FutureEvents.Count > 0)
                {
                    //  ViewBag.NextGrubClub = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).OrderBy(x => x.Start).FirstOrDefault();
                    ViewBag.FutureEventCount = model.FutureEvents.Where(x => ((x.Private == false) && (x.Status == (int)EventStatus.Active))).Count();

                }
                else
                {
                    ViewBag.FutureEventCount = 0;
                }

                if (UserMethods.IsLoggedIn)
                {
                    //  ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
                    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(supperClubId, UserMethods.CurrentUser.Id);

                }

                if (!string.IsNullOrEmpty(model.Twitter))
                    model.Twitter = "@" + model.Twitter;
                vmodel.SupperClub = model;
                if (model.FutureEvents != null && model.FutureEvents.Count > 0)
                {
                    //  if (model.FutureEvents != null)
                    //     ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(ViewBag.NextGrubClub.Id, UserMethods.CurrentUser.Id);

                    ViewBag.NextGrubClub = model.FutureEvents.OrderBy(x => x.Start).First();
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



                return View(vmodel);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        #endregion

        #region private methods

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
                try
                {
                    listReviews.Add(new
                    {
                        createdDate = tempReview.DateCreated.ToString("ddd, d MMM yyyy"),
                        rating = tempReview.Rating == null ? 0 : tempReview.Rating,
                        review = tempReview.PublicReview,
                        Name = tempReview.User != null ? tempReview.User.FirstName : ""

                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return listReviews;
        }

        private List<object> GetSupperClubFollowers(Domain.SupperClub model)
        {
            var listUsers = new List<object>();
            IList<User> lstUsers = _supperClubRepository.GetSupperClubFollowersList(model.Id);
            // IList<User> lstUsers = _supperClubRepository.GetSupperClubFollowersList(173);

            for (int i = 0; i < lstUsers.Count; i++)
            {
                var _user = lstUsers[i];

                listUsers.Add(new
                {
                    UserName = _user.FirstName + " " + _user.LastName,
                    EmailId = _user.Email
                });
            }

            return listUsers;

        }
        private List<object> GetFutureEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            List<Event> lstEvents = model.FutureEvents.Where(x => ((x.Status == (int)EventStatus.Active))).OrderBy(x => x.Start).ToList<Event>();
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];

                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = model.AverageRank == null ? 0 : (int)model.AverageRank,//tempEvent.AverageRank== null?0 : tempEvent.AverageRank,
                    reviewsCount = model.NumberOfReviews,//tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
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
            List<Event> lstEvents = model.PastEvents.Where(x => ((x.Status == (int)EventStatus.Active))).OrderByDescending(x => x.Start).ToList<Event>();

            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    status = tempEvent.Status,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            return listEvents;
        }
        private List<object> GetPrivateEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            //for (int i = 0; i < model.PastEvents.Count; i++)
            List<Event> lstEvents = model.PastEvents.Where(x => ((x.Private == true))).OrderByDescending(x => x.Start).ToList<Event>();
            List<Event> lstEventsFuture = model.FutureEvents.Where(x => ((x.Private == true))).OrderByDescending(x => x.Start).ToList<Event>();
            for (int i = 0; i < lstEventsFuture.Count; i++)
            {
                var tempEvent = lstEventsFuture[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            return listEvents;
        }

        private List<object> GetRejectedAndCancelEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            //for (int i = 0; i < model.PastEvents.Count; i++)
            List<Event> lstEvents = model.PastEvents.Where(x => ((x.Status == (int)EventStatus.Rejected || x.Status == (int)EventStatus.Cancelled))).OrderByDescending(x => x.Start).ToList<Event>();
            List<Event> lstEventsFuture = model.FutureEvents.Where(x => ((x.Status == (int)EventStatus.Rejected || x.Status == (int)EventStatus.Cancelled))).OrderByDescending(x => x.Start).ToList<Event>();
            for (int i = 0; i < lstEventsFuture.Count; i++)
            {
                var tempEvent = lstEventsFuture[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    status = tempEvent.Status,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    status = tempEvent.Status,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            return listEvents;
        }

        private List<object> GetNewEvents(Domain.SupperClub model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            int SupperclubEventCount = _supperClubRepository.GetAllEventsForASupperClub(model.Id).Count();
            //for (int i = 0; i < model.PastEvents.Count; i++)
            List<Event> lstEvents = model.PastEvents.Where(x => ((x.Status == (int)EventStatus.New))).OrderBy(x => x.Start).ToList<Event>();
            List<Event> lstEventsFuture = model.FutureEvents.Where(x => ((x.Status == (int)EventStatus.New))).OrderBy(x => x.Start).ToList<Event>();
            for (int i = 0; i < lstEventsFuture.Count; i++)
            {
                var tempEvent = lstEventsFuture[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    totalGuests = tempEvent.TotalEventGuests,
                    availableSeats = tempEvent.TotalNumberOfAvailableSeats,
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank == 0) ? (model.AverageRank == null ? 0 : (int)model.AverageRank) : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.Reviews == null || tempEvent.Reviews.Count() == 0) ? model.NumberOfReviews : tempEvent.Reviews.Count(),
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    chefUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                    id = tempEvent.Id,
                    reviewUrl = tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "#reviews",
                    brandNew = (SupperclubEventCount <= 2) ? "1" : "0",
                    isPrivate = tempEvent.Private,
                    seatsSold = tempEvent.TotalEventGuests - tempEvent.TotalNumberOfAvailableSeats
                });
            }
            return listEvents;
        }
        private List<object> GetReviews(Domain.User model)
        {
            var listReviews = new List<object>();

            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Review> lstReviews=model.Reviews.OrderByDescending(y=>y.DateCreated).ToList();
            for (int i = 0; i < lstReviews.Count; i++)
            // for (int i=0; i<model.Reviews.Count();i++)
            {
                var tempReview = lstReviews[i];
                try
                {
                    listReviews.Add(new
                    {
                        createdDate = tempReview.DateCreated.ToString("ddd, d MMM yyyy"),
                        rating = tempReview.Rating == null ? 0 : tempReview.Rating,
                        review = tempReview.PublicReview,
                        Name = tempReview.Event.Name
                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return listReviews;
        }
        private List<object> GetFutureEvents(Domain.User model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Event> lstEvents = model.FutureEvents.OrderBy(x => x.Start).ToList();
            for (int i = 0; i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yyyy"),                    
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),                    
                    averageRank = tempEvent.SupperClub.AverageRank,//tempEvent.AverageRank == null ? 0 : tempEvent.AverageRank,
                    reviewsCount = tempEvent.SupperClub.NumberOfReviews,
                    reviewUrl ="/"+ tempEvent.SupperClub.UrlFriendlyName+"/#reviews"
                });
            }
            return listEvents;
        }
        private List<object> GetPastEvents(Domain.User model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            List<Event> lstEvents = model.PastEvents.OrderByDescending(x => x.Start).ToList();
            for (int i = 0; i < 10 && i < lstEvents.Count; i++)
            {
                var tempEvent = lstEvents[i];
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yyyy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? tempEvent.SupperClub.AverageRank : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? tempEvent.SupperClub.NumberOfReviews : tempEvent.Reviews.Count(),
                    reviewUrl = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? "/" + tempEvent.SupperClub.UrlFriendlyName + "/#reviews" : "/" + tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "/#reviews"
                });
            }
            return listEvents;
        }
        private List<object> GetWishListedEvents(Domain.User model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //for (int i = 0; i < model.PastEvents.Count; i++)

            for (int i = 0; i < model.WishlistedEvents.Count; i++)
            {
                var tempEvent = model.WishlistedEvents[i].Event;
                listEvents.Add(new
                {
                    eventId = tempEvent.Id,
                    name = tempEvent.Name,
                    urlFriendlyName = tempEvent.UrlFriendlyName,
                    startDateTime = tempEvent.Start.ToString("ddd, d MMM yyyy"),
                    ticketCost = tempEvent.CostToGuest.ToString(),
                    imageURL = tempEvent.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),                    
                    averageRank = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? tempEvent.SupperClub.AverageRank : tempEvent.AverageRank,
                    reviewsCount = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? tempEvent.SupperClub.NumberOfReviews : tempEvent.Reviews.Count(),
                    reviewUrl = (tempEvent.AverageRank == null || tempEvent.AverageRank==0) ? "/" + tempEvent.SupperClub.UrlFriendlyName + "/#reviews" : "/" + tempEvent.UrlFriendlyName + "/" + tempEvent.Id + "/#reviews"
                });
            }
            return listEvents;
        }
        private List<object> GetFollowedSupperClubs(Domain.User model)
        {
            var listEvents = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            //for (int i = 0; i < model.PastEvents.Count; i++)

            for (int i = 0; i < model.FollowedSupperClubs.Count; i++)
            {
                var tempSC = model.FollowedSupperClubs[i].SupperClub;
                listEvents.Add(new
                {
                    eventId = tempSC.Id,
                    name = tempSC.Name,
                    urlFriendlyName = tempSC.UrlFriendlyName,
                    imageURL = tempSC.ImagePath,// Url.Content(ServerMethods.EventImagePath + tempEvent.ImagePath),                    
                    averageRank = tempSC.AverageRank == null ? 0 : tempSC.AverageRank,
                    reviewsCount = tempSC.NumberOfReviews
                });
            }
            return listEvents;
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
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}

