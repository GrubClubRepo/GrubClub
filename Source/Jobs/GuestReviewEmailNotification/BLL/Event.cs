using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuestReviewEmailNotification.BLL
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }        
        public string UrlFriendlyName { get; set; }
        public string SupperClubUrlFriendlyName { get; set; }
        public int SupperClubId { get; set; }
        //public string SupperClubFirstName { get; set; }
        //public string SupperClubLastName { get; set; }
        //public string ImagePath { get; set; }
        public decimal Cost { get; set; }
        public string City { get; set; } 
    }

}
