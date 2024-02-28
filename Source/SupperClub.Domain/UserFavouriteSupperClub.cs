using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class UserFavouriteSupperClub
    {
        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int SupperClubId { get; set; }
        public Nullable<DateTime> CreateDate { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        [ForeignKey("SupperClubId")]
        public virtual SupperClub SupperClub { get; set; }
    }
}
