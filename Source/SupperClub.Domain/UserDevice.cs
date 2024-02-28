using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class UserDevice
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
        public bool FbFriendInstalledApp { get; set; }
        public bool FbFriendBookedTicket { get; set; }
        public bool FavChefNewEventNotification { get; set; }
        public bool FavEventBookingReminder { get; set; }
        public bool WaitlistEventTicketsAvailable { get; set; }  
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
