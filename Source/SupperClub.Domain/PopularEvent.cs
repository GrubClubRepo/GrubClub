using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public enum PopularEventStatus
    {
        Active = 1,
        Deactivated = 2,
        New = 3
    }

    public class PopularEvent
    {
        #region Properties

        public int Id { get; set; }
        public int EventId { get; set; }
        public int Rank { get; set; }
        public int Status { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        #endregion

    }
}
