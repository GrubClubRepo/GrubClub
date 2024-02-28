using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class BraintreeCustomer
    {
        public string BraintreeCustomerId { get; set; }
        public Guid UserId { get; set; }

        //[ForeignKey("UserId")]
        //public virtual User User { get; set; }
    }
}
