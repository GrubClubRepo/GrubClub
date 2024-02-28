using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChefNewEventNotificationService.BLL
{
    public class CostCalculator
    {
        public static decimal vatPercent = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["VATPercent"]);
        public static decimal commissionPercent = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["CommissionPercent"]);
        public static decimal commissionFixed = decimal.Parse(System.Configuration.ConfigurationManager.AppSettings["CommissionFixed"]);

        public static decimal vatMultiplier = vatPercent / 100;
        //   public static decimal commissionMultiplier = commissionPercent / 100;

        private static decimal TotalFee(decimal basePrice, decimal commissionPercent)
        {
            decimal commissionMultiplier = commissionPercent / 100;
            // Total in fees
            return (basePrice * commissionMultiplier) + commissionFixed;
        }

        public static decimal CommissionTotal(decimal basePrice, decimal commissionPercent)
        {
            // VAT is taken out of total fee to leave the commission
            return TotalFee(basePrice, commissionPercent) / (1 + vatMultiplier);
        }

        public static decimal VATTotal(decimal basePrice, decimal commissionPercent)
        {
            // VAT is taken from the total commission
            return CommissionTotal(basePrice, commissionPercent) * vatMultiplier;
        }

        public static decimal CostToGuest(decimal basePrice, decimal commissionPercent)
        {
            // Base ticket price + commission + VAT on commission
            return Math.Round(basePrice + CommissionTotal(basePrice, commissionPercent) + VATTotal(basePrice, commissionPercent), 2);
        }
    }
}
