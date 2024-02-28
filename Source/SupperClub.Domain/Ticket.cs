using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class Ticket
    {
        public Ticket()
        {
        }

        public Ticket(int eventId, decimal basePrice, Guid basketId, Guid userId, int seatingId, int menuOptionId,  decimal commission,int voucherId = 0, decimal discount = 0)
        {
            EventId = eventId;
            UserId = userId;
            BasketId = basketId;
            SeatingId = seatingId;
            MenuOptionId = menuOptionId;

            BasePrice = basePrice;
            VoucherId = voucherId;
            DiscountAmount = discount;

            CommissionFixed = CostCalculator.commissionFixed;
            CommissionMultiplier = commission; // CostCalculator.commissionMultiplier;
            CommissionTotal = CostCalculator.CommissionTotal(basePrice,commission);

            VATMultiplier = CostCalculator.vatMultiplier;
            VATTotal = CostCalculator.VATTotal(basePrice,commission);
            
            TotalPrice = CostCalculator.CostToGuest(basePrice,commission);
        }

        public int Id { get; set; }
        public Guid UserId { get; set; }
        public int EventId { get; set; }
        public Guid BasketId { get; set; }
        public int SeatingId { get; set; }
        public int MenuOptionId { get; set; }
        public int VoucherId { get; set; }
        public decimal DiscountAmount { get; set; }

        public decimal BasePrice { get; set; }
        
        public decimal CommissionMultiplier { get; set; }
        public decimal CommissionFixed { get; set; }
        public decimal CommissionTotal { get; set; }

        public decimal VATMultiplier { get; set; }
        public decimal VATTotal { get; set; }

        public decimal TotalPrice { get; set; }
        public string Description { get; set; }

        //public virtual User User { get; set; }
        [ForeignKey("BasketId")]
        public virtual TicketBasket TicketBasket { get; set; }
        [ForeignKey("SeatingId")]
        public virtual EventSeating EventSeating { get; set; }
        [ForeignKey("MenuOptionId")]
        public virtual EventMenuOption EventMenuOption { get; set; }
    }
}
