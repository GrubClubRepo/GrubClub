using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class EventWaitList
    {       
        public int Id { get; set; }
        public Guid? UserId { get; set; }
        public int EventId { get; set; }
        public string Email { get; set; }
        public bool NotificationSent { get; set; }
        public DateTime AddedDate { get; set; }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        
        public virtual User User { get; set; }
    }
}
