using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SupperClub.Models;
using log4net;
using System.Web.Configuration;
using SupperClub.Code;
using SupperClub.WebUI;
using SupperClub.Logger;

namespace SupperClub.Controllers
{
    //[SupperClub.Web.Helpers.Utils.ExitHttpsIfNotRequired]
    public abstract class BaseController : Controller
    {
        protected ISupperClubRepository _supperClubRepository;
        protected static readonly ILog log = LogManager.GetLogger(typeof(Controller));
        protected SegmentMethod _segmentMethod = new SegmentMethod();

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            // Log any exceptions
            if (filterContext.Exception != null)
            {
                WebUILogging.LogException("Action Unhandled Exception: " + filterContext.Exception.Message + "    STACK_TRACE: " + filterContext.Exception.StackTrace, filterContext.Exception, log);
            }
        }

        protected void SetNotification(NotificationType type, string text, bool closeable = false, bool hideMainContainer = false, bool noContainer = false)
        {
            ViewBag.HideMainContainer = hideMainContainer;
            NotificationModel nModel = new NotificationModel(type, text, closeable, hideMainContainer, noContainer);
            ViewBag.Notification = nModel;
            TempData["Notification"] = nModel;
        }

        protected static void LogException(string message, Exception ex)
        {
            WebUILogging.LogException(message, ex, log);
        }

        protected static void LogMessage(string message, LogLevel level = LogLevel.INFO)
        {
            WebUILogging.LogMessage(message, log, level);
        }

        protected static void LogLongMessage(string shortMessage, string longMessage, LogLevel level = LogLevel.INFO)
        {
            WebUILogging.LogLongMessage(shortMessage, longMessage, log, level);
        }
    }
}
