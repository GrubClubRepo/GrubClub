using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class UserFacebookFriend
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public string UserFacebookId { get; set; }
        public string FriendFacebookId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }       
    }
}
