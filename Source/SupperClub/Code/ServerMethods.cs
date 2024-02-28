using System;
using System.Web;
using System.Web.Configuration;

namespace SupperClub.Code
{
    /// <summary>
    /// Represents server methods.
    /// </summary>
    public class ServerMethods
    {
        /// <summary>
        /// Gets the server URL.
        /// </summary>
        /// <value>the server URL.</value>
        public static string ServerUrl
        {
            get
            {
                return HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + VirtualPathUtility.ToAbsolute("~/");
            }
        }
        /// <summary>
        /// Gets the Sitemap Server URL.
        /// </summary>
        /// <value>Sitemap Server URL</value>
        public static string SiteMapServerUrl
        {
            get
            {
                return ServerUrl + "sitemap/";
            }
        }

        public static string TempImagePath
        {
            get
            {
                return WebConfigurationManager.AppSettings["TempImagePath"];
            }
        }

        public static string EventImagePath
        {
            get
            {
                return WebConfigurationManager.AppSettings["EventImagePath"];
            }
        }

        public static string SupperClubImagePath
        {
            get
            {
                return WebConfigurationManager.AppSettings["SupperClubImagePath"];
            }
        }
        public static string TempImagePathPartial
        {
            get
            {
                return WebConfigurationManager.AppSettings["TempImagePath"].Replace("~/","");
            }
        }

        public static string EventImagePathPartial
        {
            get
            {
                return WebConfigurationManager.AppSettings["EventImagePath"].Replace("~/", "");
            }
        }

        public static string SupperClubImagePathPartial
        {
            get
            {
                return WebConfigurationManager.AppSettings["SupperClubImagePath"].Replace("~/", "");
            }
        }
        public static string ImageCollectionPath
        {
            get
            {
                return WebConfigurationManager.AppSettings["ImageCollectionPath"];
            }
        }
        public static string PressReleaseImagePath
        {
            get
            {
                return WebConfigurationManager.AppSettings["PressReleaseImagePath"];
            }
        }

        public static string GoogleMapsCircleRadius
        {
            get
            {
                return WebConfigurationManager.AppSettings["GoogleMapsCircleRadius"];
            }
        }
    }
}