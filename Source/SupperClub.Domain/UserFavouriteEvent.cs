using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class UserFavouriteEvent
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int EventId { get; set; }
        public bool PushNotificationSent { get; set; }
        public bool EmailNotificationSent { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
    }
}
