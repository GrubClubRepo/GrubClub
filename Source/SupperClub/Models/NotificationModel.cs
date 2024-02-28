using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupperClub.Models
{
    public enum NotificationType
    {
        Error = 1,
        Success = 2,
        Warning = 3,
        Info = 4,
    }

    public class NotificationModel
    {
        public NotificationType Type { get; set; }
        public string Text { get; set; }
        public bool Closaeable { get; set; }
        public bool HideMainContainer { get; set; }
        public bool NoContainer { get; set; }

        public NotificationModel(NotificationType type, string text, bool closeable = false, bool hideMainContainer = false, bool noContainer = false)
        {
            this.Type = type;
            this.Text = text;
            this.Closaeable = closeable;
            this.HideMainContainer = hideMainContainer;
            this.NoContainer = noContainer;
        }
    }
}