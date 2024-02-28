using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class UserInvitee
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int SubscriberId { get; set; }
              
        [ForeignKey("SubscriberId")]
        public virtual Subscriber Subscriber { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}
