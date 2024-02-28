using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.Web.Mvc;

namespace SupperClub.Models
{
    public class GenerateReviewKeysModel
    {
        public GenerateReviewKeysModel(int eventsDaysOld)
        {
            AllowGuests = false;
            EventsDaysOld = eventsDaysOld;
        }

        public bool AllowGuests { get; set; }
        public int SelectedEventId { get; set; }
        public int EventsDaysOld { get; set; }
    }
}