using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.WebUI;
using System.Web.Configuration;
using log4net;
using SupperClub.Logger;

namespace SupperClub.Code
{
    public class WebUILogging
    {
        // Logging here allows us to grab the current User (if logged in) for all log events

        public static void LogMessage(string message, ILog log, LogLevel level = LogLevel.INFO)
        {
            string uName = string.IsNullOrEmpty(UserMethods.UserName) ? "Unknown" : UserMethods.UserName;
            SystemLogging.LogMessage(string.Format("User: {0} - {1}", uName, message), log, level);
        }

        public static void LogException(string message, Exception ex, ILog log)
        {
            string uName = string.IsNullOrEmpty(UserMethods.UserName) ? "Unknown" : UserMethods.UserName;
            SystemLogging.LogException(string.Format("User: {0} - {1}", uName, message), ex, log);
        }

        public static void LogLongMessage(string shortMessage, string longMessage, ILog log, LogLevel level = LogLevel.INFO)
        {
            string uName = string.IsNullOrEmpty(UserMethods.UserName) ? "Unknown" : UserMethods.UserName;
            SystemLogging.LogLongMessage(string.Format("User: {0} - {1}", uName, shortMessage), longMessage, log, level);
        }
    }
}