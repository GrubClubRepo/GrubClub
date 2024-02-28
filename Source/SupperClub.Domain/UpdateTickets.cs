using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace SupperClub.Domain
{
    public class UpdateTicketsResult
    {
        public bool Success { get; set; }
        public int NumberTicketsAllocated { get; set; }
        public int NumberTicketsAvailable { get; set; }
        public decimal DiscountAmount { get; set; }

        public UpdateTicketsResult()
        {
            Success = false;
            NumberTicketsAllocated = 0;
            NumberTicketsAvailable = 0;
            DiscountAmount = 0;
        }
    }
    public class ApplyVoucherCodeResult
    {
        public int Status { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Discount { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAfterDiscount { get; set; }
        public string VoucherDescription { get; set; }
    }
    public class CheckSeatingOverlap
    {
        public bool Exists { get; set; }
        
        public CheckSeatingOverlap()
        {
            Exists = false;
        }
    }

    public class GetCostToGuest
    {
        public decimal DisplayCost { get; set; }

        public GetCostToGuest()
        {
            DisplayCost = 0;
        }
    }

}
