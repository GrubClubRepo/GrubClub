using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChefNewEventNotificationService.BLL
{
    public class User
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }    
    }    

}
