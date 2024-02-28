using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Ninject;
using Ninject.Web.Common;
using SupperClub.Services;
using SupperClub.Domain.Repository;
using SupperClub.Data;
using SupperClub.Web.Infrastructure;
using SupperClub.Data.EntityFramework;
using log4net;
using SagePayMvc;
using System.Reflection;
using System.Web.Configuration;
using Braintree;
using Segment;

namespace SupperClub
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("blog/{*pathInfo}");
            routes.IgnoreRoute("notesfromasupperclub/{*pathInfo}");
            routes.IgnoreRoute("notes-from-a-pop-up-restaurant/{*pathInfo}");
            routes.Ignore("notes-from-a-pop-up-restaurant");

            // Try home test page link           
            routes.MapRoute("HomeTestRoute",
                "home",
                new { controller = "Home", action = "HomeTest" }
            );
            // Try category link           
            routes.MapRoute("CategoryRoute",
                "{categoryname}",
                new { controller = "Search", action = "CategoryDetailsByName" },
                new { categoryname = new MustMatchSearchCategory() }
            );
            // Try tags city area  link          
            routes.MapRoute("TagCityAreaSearchRoute",
                "{categoryname}/{tagname}/{cityname}/{areaname}",
                new { controller = "Search", action = "TagCityAreaSearchResult" },
                new { categoryname = new MustMatchSearchCategory(), tagname = new MustMatchTag(), cityname = new MustMatchCity(), areaname = new MustMatchArea() }
            );
            // Try tags city  link          
            routes.MapRoute("TagCitySearchRoute",
                "{categoryname}/{tagname}/{cityname}",
                new { controller = "Search", action = "TagCitySearchResult" },
                new { categoryname = new MustMatchSearchCategory(), tagname = new MustMatchTag(), cityname = new MustMatchCity() }
            );
            // Try category tags link           
            routes.MapRoute("CategorySearchRoute",
                "{categoryname}/{tagname}",
                new { controller = "Search", action = "CategorySearchResult" },
                new { categoryname = new MustMatchSearchCategory(), tagname = new MustMatchTag() }
            );
            // Try tags  link          
            routes.MapRoute("TagSearchRoute",
                "{tagname}",
                new { controller = "Search", action = "TagSearchResult" },
                new { tagname = new MustMatchTag() }
            );          
            
            // Try Supper Club quick links
            routes.MapRoute("SCQuickLinkRoute",
                "{hostname}",
                new { controller = "Host", action = "DetailsByName" },
                new { hostname = new MustMatchSupperClub() }
            );
            // Try Supper Club review link
            routes.MapRoute("SCReviewLinkRoute",
                "{hostname}/allreviews",
                new { controller = "Host", action = "ReviewsByName", loadSummary = false },
                new { hostname = new MustMatchSupperClub(), loadSummary = new MustMatchBool() }
            );
            // Try Supper Club review link
            routes.MapRoute("SCReviewRoute",
                "{hostname}/reviews",
                new { controller = "Host", action = "ReviewsByName", loadSummary = true },
                new { hostname = new MustMatchSupperClub() }
            );
            // Experiences page link
            routes.MapRoute("ExperiencesRoute",
                "experiences",
                new { controller = "Home", action = "experiences" }
            );
            // Contact us page link
            routes.MapRoute("ContactUsRoute",
                "contact-us",
                new { controller = "Home", action = "ContactUs" }
            );
            // Gift Voucher page link
            routes.MapRoute("VocuherRoute",
                "gift-voucher",
                new { controller = "Home", action = "GiftVoucher" }
            );
            // Referral Voucher page link
            routes.MapRoute("ReferralVoucherRoute",
                "referral-voucher",
                new { controller = "Home", action = "ReferralVoucher" }
            );
            // Contact us page link
            routes.MapRoute("ChefRoute",
                "chefs",
                new { controller = "Home", action = "Chefs" }
            );
            
            //How it works chef page link
            routes.MapRoute("HowItWorksChefRoute",
                "how-to-set-up-a-grub-club",
                new { controller = "Home", action = "HowItWorksHosts" }
            );
            routes.MapRoute("FoodiesRoute",
               "foodies",
               new { controller = "Home", action = "Foodies" }
          
            );
            //chef faq page link
            routes.MapRoute("FAQChefRoute",
                "how-to-set-up-a-pop-up-restaurant",
                new { controller = "Home", action = "FAQsHost" }
            );
            //Chef HowToHost page link
            routes.MapRoute("HowToHostChefRoute",
                "how-to-set-up-a-supperclub",
                new { controller = "Home", action = "HowToHost" }
            );
            // Try regular quick links
            routes.MapRoute("QuickLinkRoute",
                "{viewname}",
                new { controller = "Home", action = "ViewQuickLink" },
                new { viewname = new MustMatchPage() }
            );
            // Try old quick links
            routes.MapRoute("QuickLinkRouteOld",
                "home/{viewname}",
                new { controller = "Home", action = "ViewQuickLinkOld" },
                new { viewname = new MustMatchPage() }
            );
            // About us page old link redirect
            routes.MapRoute("FoodiesRouteOld",
                "home/foodies",
                new { controller = "Home", action = "FoodiesRedirect" }
            );

            // About us page old link redirect
            routes.MapRoute("AboutUsRoute",
                "home/aboutus",
                new { controller = "Home", action = "AboutUsRedirect"}
            );
            // How it works page old link redirect
            routes.MapRoute("HowItWorksRoute",
                "home/howitworks",
                new { controller = "Home", action = "HowItWorksRedirect" }
            );
            // Refer A Friend page old link redirect
            routes.MapRoute("ReferAFriendRoute",
                "home/referafriend",
                new { controller = "Home", action = "ReferAFriendRedirect" }
            );
            // Contact us page old link redirect
            routes.MapRoute("ContactUsRouteOld",
                "home/contactus",
                new { controller = "Home", action = "ContactUsRedirect" }
            );
            // Gift Voucher page old link redirect
            routes.MapRoute("GiftVoucherRouteOld",
                "home/giftvoucher",
                new { controller = "Home", action = "GiftVoucherRedirect" }
            );
            // Referral Voucher page old link redirect
            routes.MapRoute("ReferralVoucherRouteOld",
                "home/referralvoucher",
                new { controller = "Home", action = "ReferralVoucherRedirect" }
            );
            // Chefs page old link redirect
            routes.MapRoute("ChefsRouteOld",
                "home/chefs",
                new { controller = "Home", action = "ChefsRedirect" }
            );
            // Chefs working with grub club page
            routes.MapRoute("ChefWorkingWithGrubClubRoute",
                "working-with-grubclub",
                new { controller = "Host", action = "WorkingWithGrubclub" }
            );
            // Chefs what we list page
            routes.MapRoute("ChefWhatWeListRoute",
                "what-we-list-on-grubclub",
                new { controller = "Host", action = "WhatWeList" }
            );
            // Chefs Setting up a Grub Club page
            routes.MapRoute("ChefSettingUpAGrubClubRoute",
                "how-to-start-a-grubclub",
                new { controller = "Host", action = "SettingUpAGrubClub" }
            );
            // Chefs what we list page
            routes.MapRoute("ChefUploadingGrubClubRoute",
                "how-to-manage-your-grubclub",
                new { controller = "Host", action = "UploadingOntoGrubClub" }
            );
            // Email Signup page link
            routes.MapRoute("EmailSignupRoute",
                "signup",
                new { controller = "Home", action = "EmailSignup" }
            );
            // old event quick links
            routes.MapRoute("EventGenericRoute",
                "{hostname}/{eventSeoFriendlyName}",
                new { controller = "Event", action = "MasterDetailsByName" },
                new { hostname = new MustMatchSupperClub(), eventSeoFriendlyName = new MustMatchEventUrl() }
            );
            // old event quick links
            routes.MapRoute("EventOldLinkRoute",
                "{eventSeoFriendlyName}/{eventId}",
                new { controller = "Event", action = "DetailsByIdAndName" },
                new { eventSeoFriendlyName = new MustMatchEventUrl(), eventId = new MustMatchEventId() }
            );
            // Try event quick links
            routes.MapRoute("EventLinkRoute",
                "{hostname}/{eventSeoFriendlyName}/{eventId}",
                new { controller = "Event", action = "DetailsByIdWithName" },
                new { hostname = new MustMatchSupperClub(), eventSeoFriendlyName = new MustMatchEventUrl(), eventId = new MustMatchEventId() }
            );
            // Try alternate event link for old url redirection
            routes.MapRoute("EventLinkRedirectRoute",
                "{hostname}/{eventSeoFriendlyName}/{eventId}",
                new { controller = "Event", action = "DetailsByIdWithName" },
                new { eventId = new MustMatchEventId() }
            );

            // Search city event result link
            routes.MapRoute(
              "SearchCityAreaResultLinkRoute",
              "pop-up-restaurants/{cityname}/{areaname}",
              new { controller = "Search", action = "SearchCityAreaResult" },
              new { cityname = new MustMatchCity(), areaname = new MustMatchArea() }
            );
            // Search city event result link
            routes.MapRoute(
              "SearchCityResultLinkRoute",
              "pop-up-restaurants/{cityname}",
              new { controller = "Search", action = "SearchCityResult" },
              new { cityname = new MustMatchCity() }
            );
            routes.MapRoute(
             "SearchLinkRoute",
             "pop-up-restaurants",
             new { controller = "Search", action = "Index" }
           );  
            // Search event result link
            routes.MapRoute(
              "SearchResultLinkRoute",
              "pop-up-restaurants", new { controller = "Search", action = "SearchResult" }
            );
            // Search event link
            routes.MapRoute(
              "SearchOldLinkRoute",
              "search/london/pop-up-restaurants",
              new { controller = "Search", action = "RedirectSearch" }
            );
            
            // Search event link
            routes.MapRoute(
              "SearchAltLinkRoute",
              "search/london",
              new { controller = "Search", action = "RedirectSearch1" }
            );
            // Search event result link
            routes.MapRoute(
              "HomeSearchResultLinkRoute",
              "search/london/popup-restaurants", new { controller = "Search", action = "HomeSearchResult" }
            );
            // New Home Page Search event result link
            routes.MapRoute(
              "NewHomeSearchResultLinkRoute",
              "search/london/popup-restaurant", new { controller = "Search", action = "NewHomeSearchResult" }
            );


            // Try regular routes
            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

            // 404 Route not found

        }

        public void SetupDependencyInjection()
        {
            // Create Ninject DI kernel
            IKernel kernel = new StandardKernel();

            //kernel.Bind<SupperClubContext>().ToSelf().InSingletonScope();
            // Register services with Ninject DI Container
            kernel.Bind<ISupperClubRepository>().To<SupperClubRepository>().InRequestScope();
            kernel.Bind<ITransactionService>().To<TransactionService>();

            // The following types are defined in the SagePayMvc library and PayPalMvc library itself.
            kernel.Bind<SagePayMvc.ITransactionRegistrar>().To<SagePayMvc.TransactionRegistrar>();
            kernel.Bind<SagePayMvc.IHttpRequestSender>().To<SagePayMvc.HttpRequestSender>();
            kernel.Bind<SagePayMvc.IUrlResolver>().To<SagePayMvc.DefaultUrlResolver>();
            kernel.Bind<SagePayMvc.Configuration>().ToMethod(x => SagePayMvc.Configuration.Current);

            kernel.Bind<PayPalMvc.ITransactionRegistrar>().To<PayPalMvc.TransactionRegistrar>();
            kernel.Bind<PayPalMvc.IHttpRequestSender>().To<PayPalMvc.HttpRequestSender>();
            kernel.Bind<PayPalMvc.Configuration>().ToMethod(x => PayPalMvc.Configuration.Current);

            // Tell ASP.NET MVC 3 to use our Ninject DI Container
            DependencyResolver.SetResolver(new NinjectDependencyResolver(kernel));
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(MvcApplication));

        void Application_Error(Object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError().GetBaseException();
            bool is404 = (ex.Message.Contains("was not found or does not implement IController") || ex.Message.Contains("was not found on controller"));
            if (is404) // Only log 404's as a warning
                log.Warn(string.Format("User: {0} -> Warning: {1}", (string.IsNullOrEmpty(User.Identity.Name)) ? "Unknown" : User.Identity.Name, ex.Message), ex);
            else
                log.Error(string.Format("User: {0} -> Error: {1}", (string.IsNullOrEmpty(User.Identity.Name)) ? "Unknown" : User.Identity.Name, ex.Message + ex.InnerException + ex.StackTrace), ex);
        }

        protected void Application_Start()
        {
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeBinder());
            //GlobalFilters.Filters.Add(new RequireHttpsAttribute());
            SetupDependencyInjection();
            //ControllerBuilder.Current.SetControllerFactory(new NinjectControllerFactory());
            AreaRegistration.RegisterAllAreas();
            string segmentKey = WebConfigurationManager.AppSettings["SegmentKey"];
            Segment.Analytics.Initialize(segmentKey);
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            log4net.Config.XmlConfigurator.Configure();
        }

        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            //if (!Request.IsLocal)
            //{
            //    switch (Request.Url.Scheme)
            //    {
            //        case "https":
            //            Response.AddHeader("Strict-Transport-Security", "max-age=300");
            //            break;
            //        case "http":
            //            var path = "https://" + Request.Url.Host + Request.Url.PathAndQuery;
            //            Response.Status = "301 Moved Permanently";
            //            Response.AddHeader("Location", path);
            //            break;
            //    }
            //}
            //Declare the server URL ex:www.mysite.com 
            string server = Request.ServerVariables["SERVER_NAME"];
            //Declare the form being accessed ex: Default.aspx 
            string url = Request.ServerVariables["URL"];
            // Declare the query string in the URL 
            string querystring = Request.ServerVariables["QUERY_STRING"];
            // Protocol
            string protocol = Request.ServerVariables["HTTPS"];
            // Merge the server name with the form ex: www.mysite.com/Default.aspx 
            string fullurl = server + url;
            // Create a string for any URL with a querystring ex: ?&categoryid=1 
            string tail = "?" + querystring;
            // Declare string you want replaced ex: www. 
            string patternwww = "www.";
            // Declare what you want it replaced with 
            string patternclear = string.Empty;
            if (!(fullurl.Contains("notes-from-a-pop-up-restaurant") || fullurl.Contains("notesfromasupperclub") || fullurl.Contains("grubclub.com/blog") || fullurl.Contains("grubclub.com/pop-up-christabel")))
            {
                // Redirect grubclub.com/elements-by-kitchen-theory to grubclub.com/kitchen-theory
                if (fullurl.ToLower().Contains("grubclub.com/elements-by-kitchen-theory"))
                //if (fullurl.ToLower().Contains("localhost:3628/elements-by-russian-revels"))
                {
                    // Replace www. with nothing 
                    string wwwrpl = fullurl.Replace(patternwww, patternclear);
                    // Replace elements-by-kitchen-theory with kitchen-theory
                    wwwrpl = wwwrpl.Replace("elements-by-kitchen-theory", "kitchen-theory");
                    //wwwrpl = wwwrpl.Replace("elements-by-russian-revels", "russian-revels");
                    // Build a string for the final URL 
                    string targeturl = ("https") + "://" + wwwrpl;
                    // Create the 301 Redirect 
                    Response.Clear();
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", targeturl);
                    Response.End();
                }

                // Create an if statement for URL's containing the string you dont want and containg query strings. 
                if (fullurl.Contains(patternwww) & querystring != null)
                {
                    // Replace www. with nothing 
                    string wwwrpl = fullurl.Replace(patternwww, patternclear);
                    // Build a string for the final URL 
                    string targeturl = ("https") + "://" + wwwrpl + tail;
                    // Create the 301 Redirect 
                    Response.Clear();
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", targeturl);
                    Response.End();

                }
                // Create an if statement for URL's containing the string that don't contain query strings 
                if (fullurl.Contains(patternwww) & querystring == null)
                {
                    // Replace www. with nothing 
                    string wwwrpl = fullurl.Replace(patternwww, patternclear);
                    // Build a string for the final URL 
                    string targeturl = ("https") + "://" + wwwrpl;
                    // Create the 301 Redirect 
                    Response.Clear();
                    Response.Status = "301 Moved Permanently";
                    Response.AddHeader("Location", targeturl);
                    Response.End();
                }
            }
        }

        public void Application_End()
        {
            // Attempt to log any Application Pool Recycles and why
            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember("_theRuntime",
                                                                                            BindingFlags.NonPublic
                                                                                            | BindingFlags.Static
                                                                                            | BindingFlags.GetField,
                                                                                            null,
                                                                                            null,
                                                                                            null);

            if (runtime == null)
                return;

            string shutDownMessage = (string)runtime.GetType().InvokeMember("_shutDownMessage",
                                                                             BindingFlags.NonPublic
                                                                             | BindingFlags.Instance
                                                                             | BindingFlags.GetField,
                                                                             null,
                                                                             runtime,
                                                                             null);

            string shutDownStack = (string)runtime.GetType().InvokeMember("_shutDownStack",
                                                                           BindingFlags.NonPublic
                                                                           | BindingFlags.Instance
                                                                           | BindingFlags.GetField,
                                                                           null,
                                                                           runtime,
                                                                           null);
            // Seems to duplicate the message content each time, so this will trim it
            if (shutDownMessage.Length > 300)
                shutDownMessage = shutDownMessage.Substring(0, 300);
            log.Warn(String.Format("Application End - _shutDownMessage: {0}", shutDownMessage));
        }
    }

    public class MustMatchSupperClub : IRouteConstraint
    {

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                Domain.SupperClub host = sp.GetSupperClub(values[parameterName].ToString());
                if (host != null)
                    isMatch = true;
                else
                    isMatch = false;
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }

    public class MustMatchPage : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                Domain.UrlRewrite url = sp.GetUrl(values[parameterName].ToString());
                if (url != null)
                    isMatch = true;
                else
                    isMatch = false;
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchEventUrl : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                Domain.Event event1 = sp.GetEventByName(values[parameterName].ToString());
                if (event1 != null)
                    isMatch = true;
                else
                    isMatch = false;
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }

    public class MustMatchEventId : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = false;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                Domain.Event event1 = sp.GetEvent(Int32.Parse(values[parameterName].ToString()));
                if (event1 != null)
                    isMatch = true;
                else
                    isMatch = false;
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchSearchCategory : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                if (values[parameterName].ToString() == null || values[parameterName].ToString() == "")
                    isMatch = true;
                else
                {
                    Domain.SearchCategory searchCategory = sp.GetSearchCategoryByName(values[parameterName].ToString());
                    if (searchCategory != null)
                        isMatch = true;
                    else
                        isMatch = false;
                }
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchTag : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                if (values[parameterName].ToString() == null || values[parameterName].ToString() == "")
                    isMatch = true;
                else
                {
                    Domain.Tag tag = sp.GetTagByName(values[parameterName].ToString());
                    if (tag != null)
                        isMatch = true;
                    else
                        isMatch = false;
                }
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchCity : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                if (values[parameterName].ToString() == null || values[parameterName].ToString() == "")
                    isMatch = true;
                else
                {
                    Domain.City city = sp.GetCityByName(values[parameterName].ToString());
                    if (city != null)
                        isMatch = true;
                    else
                        isMatch = false;
                }
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchArea : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = true;
            SupperClubRepository sp = new SupperClubRepository();
            try
            {
                if (values[parameterName].ToString() == null || values[parameterName].ToString() == "")
                    isMatch = true;
                else
                {
                    Domain.Area area = sp.GetAreaByName(values[parameterName].ToString());
                    if (area != null)
                        isMatch = true;
                    else
                        isMatch = false;
                }
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
    public class MustMatchBool : IRouteConstraint
    {
        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            bool isMatch = false;
            try
            {
                bool pValue = false;
                bool parseSuccess = bool.TryParse(values[parameterName].ToString(), out pValue);
                if (parseSuccess)
                    isMatch = true;
                else
                    isMatch = false;
            }
            catch
            {
                isMatch = false;
            }
            return isMatch;
        }
    }
}