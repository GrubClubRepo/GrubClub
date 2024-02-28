using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public enum SubscriberType
    {
        NewsLetter = 1,
        GuestInvitee = 2,
        WaitListRequest = 3,  
        SocialMedia = 4,
        ReviewEmail = 5
    }
    public class Subscriber
    {
        public virtual int Id
        {
            get;
            set;
        }
        public virtual DateTime SubscriptionDateTime { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
        public virtual int SubscriberType { get; set; }
        public virtual Guid? UserId { get; set; }
    }
}
