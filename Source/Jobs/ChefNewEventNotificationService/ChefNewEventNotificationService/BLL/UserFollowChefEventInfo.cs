using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChefNewEventNotificationService.BLL
{
    public class UserFollowChefEventInfo
    {
        public User User { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string UrlFriendlyName { get; set; }
        public string SupperClubUrlFriendlyName { get; set; }
        //public int SupperClubId { get; set; }
        public string SupperClubName { get; set; }
    }    

}
