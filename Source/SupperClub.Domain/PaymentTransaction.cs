using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string PaymentStatus { get; set; }
        public string Payment3DStatus { get; set; }
        public string PaymentStatusDetail { get; set; }
        public DateTime ResponseDate { get; set; }
        public DateTime RequestDate { get; set; }
        public string VendorTxCode { get; set; }
        public string SecurityKey { get; set; }
        public string VpsTxId { get; set; }
        public string CAVV { get; set; }
        public string RedirectUrl { get; set; }
        public DateTime DateInitialised { get; set; }
    }
}
