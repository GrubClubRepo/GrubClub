using System;
using System.Linq;
using System.Web;
using SupperClub.Models;
using SupperClub.Data;
using System.Web.Security;
using System.Collections.Generic;
using SupperClub.Domain.Repository;
using System.Web.Mvc;
using SupperClub.Domain;
using System.Net;

namespace SupperClub.Code
{
    /// <summary>
    /// Role type.
    /// </summary>
    public enum RoleTypes
    {
        Guest = 1,
        Host = 2,
        Admin = 3,
        Feed = 4,
    }

    /// <summary>
    /// Represents user methods.
    /// </summary>
    public class UserMethods : BaseModel
    {
        public static bool IsGuest
        {
            get
            {
                return RoleType == RoleTypes.Guest;
            }
        }

        public static bool IsHost
        {
            get
            {
                return SupperClubId != null;
            }
        }

        public static bool IsAdmin
        {
            get
            {
                return RoleType == RoleTypes.Admin;
            }
        }

        public static bool IsFeedUser
        {
            get
            {
                return RoleType == RoleTypes.Feed;
            }
        }
        /// <summary>
        /// Gets the type of the user.
        /// </summary>
        /// <value>
        /// The type of the user.
        /// </value>
        public static RoleTypes RoleType
        {
            get
            {
                RoleTypes roleType = RoleTypes.Guest;
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["RoleType"] == null)
                {
                    if (CurrentUser != null)
                        if (Roles.IsUserInRole(CurrentASPNETUser.UserName, "Admin"))
                            roleType = RoleTypes.Admin;
                        else if (Roles.IsUserInRole(CurrentASPNETUser.UserName, "Host"))
                            roleType = RoleTypes.Host;

                    if (HttpContext.Current.Session != null)
                        HttpContext.Current.Session["RoleType"] = roleType;
                    return roleType;
                }
                return (RoleTypes)HttpContext.Current.Session["RoleType"];
            }
        }

        /// <summary>
        /// Gets the Supper Club Id for a Host User.
        /// </summary>
        /// <value>
        /// The Supper Club Id of the Host User.
        /// </value>
        public static int? SupperClubId
        {
            get
            {
                int? supperClubId = null;
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["SupperClubId"] == null)
                {
                    if (CurrentUser != null)
                        if (Roles.IsUserInRole(CurrentASPNETUser.UserName, "Host"))
                        {
                            SupperClub.Domain.SupperClub sc = DB.GetSupperClubForUser(CurrentUser.Id);
                            supperClubId = sc.Id;
                        }

                    if (HttpContext.Current.Session != null)
                        HttpContext.Current.Session["SupperClubId"] = supperClubId;
                    return supperClubId;
                }
                return (int?)HttpContext.Current.Session["SupperClubId"];
            }
        }

        /// <summary>
        /// Gets the currently logged in user object.
        /// </summary>
        /// <returns>User object.</returns>
        public static User CurrentUser
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["User"] == null)
                {
                    if (IsLoggedIn)
                        HttpContext.Current.Session["User"] = GetSpecificUser(UserName);
                    else
                        return null;
                }
                return (User)HttpContext.Current.Session["User"];
            }
        }

        /// <summary>
        /// Gets a user object for a given username.
        /// </summary>
        /// <param name="username">The username for the user you want.</param>
        /// <returns>User object.</returns>
        public static User GetSpecificUser(string username)
        {
            Guid? id = GetSpecificUserId(username);
            if (id == null)
                return null;
            User user = DB.GetUser(id);
            return user;
        }

        /// <summary>
        /// Gets the user Id for given the currently logged in user.
        /// </summary>
        /// <returns>User Id.</returns>
        public static Guid? UserId
        {
            get
            {
                if (IsLoggedIn)
                    return CurrentUser.Id;
                else
                    return null;
            }
        }

        #region ASPNET User Membership Methods

        /// <summary>
        /// Whether user is locked.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>true if user is locked.</returns>
        public static MembershipUser GetSpecificASPNETUser(string username)
        {
            var user = Membership.GetUser(username);
            return user;
        }

        public static MembershipUser CurrentASPNETUser
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["ASPNETUser"] == null)
                {
                    if (IsLoggedIn)
                        HttpContext.Current.Session["ASPNETUser"] = GetSpecificASPNETUser(UserName);
                    else
                        return null;
                }
                return (MembershipUser)HttpContext.Current.Session["ASPNETUser"];
            }
        }

        /// <summary>
        /// Gets user id.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>User id or null if user not found.</returns>
        public static Guid? GetSpecificUserId(string username)
        {
            var user = Membership.GetUser(username);
            if (user == null)
                return null;
            return (Guid)user.ProviderUserKey;
        }

        /// <summary>
        /// Gets user email address.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>User email or null if user not found.</returns>
        public static string GetSpecificUserEmailAddress(string username)
        {
            var user = Membership.GetUser(username);
            if (user == null)
                return null;
            return user.Email;
        }

        /// <summary>
        /// Whether user is locked.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>true if user is locked.</returns>
        public static bool IsSpecificUserLockedOut(string username)
        {
            var user = Membership.GetUser(username);
            if (user == null)
                return false;
            return user.IsLockedOut;
        }

        #endregion

        #region User Settings
        public static void ClearSettingInSession(string settingName)
        {
            HttpContext.Current.Session.Remove(settingName);
        }

        public static void ClearAllSettingsInSession()
        {
            List<string> list = new List<string>();
            foreach (string s in HttpContext.Current.Session.Keys)
            {
                list.Add(s);
            }
            foreach (string s in list)
            {
                ClearSettingInSession(s);
            }
        }

        #endregion

        #region User Properties

        /// <summary>
        /// Gets the currently logged in username.
        /// </summary>
        /// <returns>User name.</returns>
        public static string UserName
        {
            get
            {
                return HttpContext.Current.User.Identity.Name;
            }
        }

        public static bool IsLoggedIn
        {
            get
            {
                return HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        public static bool IsMobileUser
        {
            get
            {
                return HttpContext.Current.Request.Browser.IsMobileDevice;
            }
        }

        public static string ClientIP
        {
            get
            {
                return HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
        }

        #endregion

        #region User Usage Events

        ///// <summary>
        ///// Add an event for a user
        ///// </summary>
        ///// <param name="userId">The user id.</param>
        ///// <param name="usageEventType">The usageEventType (enum gets converted to int)</param>
        ///// <param name="optionalNote">optional note to store</param>
        //public static void UserUsageEvent(Guid? userId, UsageEventTypes usageEventType, string optionalNote = null)
        //{
        //    if (userId != null)
        //    {
        //        UserUsageStatistic uus = new UserUsageStatistic();
        //        uus.User = DC.Users.First(x => x.Id == userId);
        //        uus.UsageEventTime = DateTime.Now;
        //        uus.UsageEventType = (int)usageEventType;
        //        uus.Note = optionalNote;
        //        uus.IsMobileEvent = IsMobileUser;
        //        DC.UserUsageStatistics.InsertOnSubmit(uus);
        //        DC.SubmitChanges();
        //    }
        //}

        #endregion   

    }

    public class SegmentMethod : BaseModel
    {
        private SupperClubRepository _supperClubRepository;
        public SegmentMethod()
        {
            _supperClubRepository = new SupperClubRepository();
        }
        #region Segment User
        public string GetSegmentUser()
        {
            string segmentUserId = "Anonymous_User";
            if (UserMethods.IsLoggedIn)
            {
                if (UserMethods.CurrentUser.SegmentUser == null || UserMethods.CurrentUser.SegmentUser.SegmentUserId == null)
                {
                    //check if a segment user Id exists for this user
                    SegmentUser segmentUser = _supperClubRepository.GetSegmentUser(UserMethods.CurrentUser.Id);
                    if (segmentUser != null)
                    {
                        UserMethods.CurrentUser.SegmentUser = segmentUser;
                        segmentUserId = segmentUser.SegmentUserId.ToString();
                    }
                    else
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
                        //su.User = UserMethods.CurrentUser;
                        // Store the UserId
                        su = _supperClubRepository.AddSegmentUser(su);
                        if (su != null)
                        {
                            UserMethods.CurrentUser.SegmentUser = su;
                            segmentUserId = su.SegmentUserId.ToString();
                        }
                    }
                }
                else
                {
                    segmentUserId = UserMethods.CurrentUser.SegmentUser.SegmentUserId.ToString();
                }
            }
            return segmentUserId;
        }
        #endregion


    }

    public class GeoMethods : BaseModel
        {
            public static GeoLocation GetGeoLocation(string postcode, string city, string address = null, string address2 = null)
            {
                GeoLocation gl = DB.GetGeoLocation(postcode, city, address, address2);
                if(gl == null)
                    gl = SupperClub.Web.Helpers.Utils.GetAddressCoordinates(postcode, city, address, address2);

                return gl;            
            }
        }
}