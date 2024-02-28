using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuestReviewEmailNotification.BLL
{
    public class User
    {
        public Nullable<Guid> UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }    
    }    

}
