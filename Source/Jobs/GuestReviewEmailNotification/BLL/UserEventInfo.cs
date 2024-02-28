using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuestReviewEmailNotification.BLL
{
    public class UserEventInfo
    {
        public int RowId { get; set; }
        public User User { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; }
        public string UrlFriendlyName { get; set; }
        public string SupperClubUrlFriendlyName { get; set; }        
        public bool IsFriend { get; set; }
    }    
}
