using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SupperClub.Domain
{
    public class SegmentUser
    {
        [Key, ForeignKey("User")]
        public Guid SegmentUserId { get; set; }
        public Guid UserId { get; set; }

        public DateTime DateCreated { get; set; }

        public virtual User User { get; set; }
    }
}
