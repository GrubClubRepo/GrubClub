using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Web;
using System.Web.Security;

namespace SupperClub.Domain
{
    public enum TicketBasketStatus
    {
        InProgress,
        Cancelled,
        Complete,
        Expired,
    }

    public class TicketBasket
    {
        
        #region Constructors
        public TicketBasket()
        {
        }

        public TicketBasket(string name)
        {
            this.Name = name;
        }
        #endregion

        #region Properties
        
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
		public string Name {get; set;}        
        public string CCLastDigits { get; set; }
        public string CCExpiryDate { get; set; }
        public string Status { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime DateCreated { get; set; }
        public int BookingReference { get; set; }
        public string BookingRequirements { get; set; }
        public bool CheckedIn { get; set; }
        public DateTime? CheckInDate { get; set; }
        
        //public virtual User User { get; set; }        
        public virtual List<Ticket> Tickets { get; set; }

        public decimal Commission { get; set; }


        #endregion

        #region Methods
        public decimal TotalPrice {
            get { return Tickets.Sum(x => x.TotalPrice); } 
		}

        public int TotalTickets
        {
            get { return Tickets.Count; }
        }

        public decimal TotalDiscount
        {
            get { return Tickets.Sum(x => x.DiscountAmount); }
        }
        #endregion
    }
}
