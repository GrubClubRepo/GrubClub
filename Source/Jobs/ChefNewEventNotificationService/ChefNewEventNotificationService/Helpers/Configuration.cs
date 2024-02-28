using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;

namespace ChefNewEventNotificationService.Helpers
{
    public static class Configuration
    {
        public static int WishListEmailTemplateId = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["WishListEmailTemplateId"]);
        public static int FollowChefEmailTemplateId = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["FollowChefEmailTemplateId"]);
        
        public static string ConnectionString = System.Configuration.ConfigurationManager.AppSettings["ConnectionString"];
        public static string OfficeAddress = System.Configuration.ConfigurationManager.AppSettings["OfficeAddress"];
        public static int Offset = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["Offset"]);
        public static string ServerURL = System.Configuration.ConfigurationManager.AppSettings["ServerUrl"];
        public static string WishListUtmParameters = System.Configuration.ConfigurationManager.AppSettings["WishListUtmParameters"];
        public static string FollowChefUtmParameters = System.Configuration.ConfigurationManager.AppSettings["FollowChefUtmParameters"];
        public static string DefaultContactEmailAddress = System.Configuration.ConfigurationManager.AppSettings["DefaultContactEmailAddress"];
        public static string AdminEmailAddress = System.Configuration.ConfigurationManager.AppSettings["AdminEmailAddress"];
        public static string InfoLogDirectory = System.Configuration.ConfigurationManager.AppSettings["InfoLogDirectory"];
        public static string ErrorLogDirectory = System.Configuration.ConfigurationManager.AppSettings["ErrorLogDirectory"];
        
        private static int _errorCounter = 0;
        public static int ErrorCounter
        {
            get { return _errorCounter; }
            set { _errorCounter = value; }
        }
        private static string _errorText = string.Empty;
        public static string ErrorText
        {
            get { return _errorText; }
            set { _errorText = value; }
        }
    }
}
