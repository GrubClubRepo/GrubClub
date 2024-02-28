using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using log4net;
using System.Text.RegularExpressions;

namespace SupperClub.Logger
{
    public enum LogLevel
    {
        DEBUG = 0,
        INFO = 1,
        WARN = 2,
        ERROR = 3,
        FATAL = 4,
        OFF = 5,
    }

    public class SystemLogging
    {
        protected static LogLevel LoggingLevel = (LogLevel)Enum.Parse(typeof(LogLevel), WebConfigurationManager.AppSettings["LoggingLevel"]);
        private static readonly ILog plog = LogManager.GetLogger(typeof(SystemLogging));

        public static void LogMessage(string message, ILog log, LogLevel level = LogLevel.INFO)
        {
            switch (level)
            {
                case LogLevel.DEBUG:
                    if (LoggingLevel <= level)
                        log.Debug(message);
                    break;
                case LogLevel.INFO:
                    if (LoggingLevel <= level)
                        log.Info(message);
                    break;
                case LogLevel.WARN:
                    if (LoggingLevel <= level)
                        log.Warn(message);
                    break;
                case LogLevel.ERROR:
                    if (LoggingLevel <= level)
                        log.Error(message);
                    break;
                case LogLevel.FATAL:
                    if (LoggingLevel <= level)
                        log.Fatal(message);
                    break;
                case LogLevel.OFF:
                    break;
            }
        }
        public static void LogLongMessage(string shortMessage, string longMessage, ILog log, LogLevel level = LogLevel.INFO)
        {
            shortMessage = shortMessage + " (see details)";

            // Split any Request/Response strings to make them readable
            if (longMessage.Contains('&'))
            {
                string[] parts = Regex.Split(longMessage, @"(?<=[&])");
                longMessage = string.Empty;
                foreach (string s in parts)
                {
                    longMessage = longMessage + s + System.Environment.NewLine;
                }
            }
            Exception ex = new Exception("(Not actually an Exception) Long Message: " + System.Environment.NewLine + longMessage);
            switch (level)
            {
                case LogLevel.DEBUG:
                    if (LoggingLevel <= level)
                        log.Debug(shortMessage, ex);
                    break;
                case LogLevel.INFO:
                    if (LoggingLevel <= level)
                        log.Info(shortMessage, ex);
                    break;
                case LogLevel.WARN:
                    if (LoggingLevel <= level)
                        log.Warn(shortMessage, ex);
                    break;
                case LogLevel.ERROR:
                    if (LoggingLevel <= level)
                        log.Error(shortMessage, ex);
                    break;
                case LogLevel.FATAL:
                    if (LoggingLevel <= level)
                        log.Fatal(shortMessage, ex);
                    break;
                case LogLevel.OFF:
                    break;
            }
        }

        public static void LogException(string message, Exception ex, ILog log)
        {
            log.Fatal(message, ex);
        }

        public static void LogMessageDirect(string message, LogLevel level = LogLevel.ERROR)
        {
            // This is used for the error page to write back that it got hit
            LogMessage(message, plog, level);
        }
    }
}
