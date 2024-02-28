using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class ActionStatus
    {
        public bool Success { get; set; }
        public string NotificationMessage { get; set; }
        public ActionStatus()
        {
            Success = false;
            NotificationMessage = "";
        }
    }
}
