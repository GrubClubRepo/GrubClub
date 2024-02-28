using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Models;

namespace SupperClub.Code
{
    public class CreditCardSample
    {
        public string CardType { get; set; }
        public string CardNumber { get; set; }
        public string Issue { get; set; }
        public int CV2 { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string Country { get; set; }

        public static CreditCardSample GetCardSample(CreditCardType crediCard)
        {
            CreditCardSample ccSample = new CreditCardSample();
            
            switch (crediCard)
            {
                case CreditCardType.AMEX:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "374200000000004", CardType = CreditCardType.AMEX.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.DELTA:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "4462000000000003", CardType = CreditCardType.DELTA.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.DINERS:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "36000000000008", CardType = CreditCardType.DINERS.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.JCB:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "3569990000000009", CardType = CreditCardType.JCB.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.LASER:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "6304990000000000044", CardType = CreditCardType.LASER.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.UKMAESTRO:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "5641820000000005", CardType = "MAESTRO", CV2 = 123, Issue = "01", PostCode = "412" };
                    break;
                case CreditCardType.INTMAESTRO:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "300000000000000004", CardType = "MAESTRO", CV2 = 123, Issue = @"N/A", PostCode = "412" };
                    break;
                case CreditCardType.MC:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "5404000000000001", CardType = CreditCardType.MC.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.UKE:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "4917300000000008", CardType = CreditCardType.UKE.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                case CreditCardType.VISA:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "4929000000006", CardType = CreditCardType.VISA.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
                default:
                    ccSample = new CreditCardSample { Address = "88", CardNumber = "5404000000000001", CardType = CreditCardType.MC.ToString(), CV2 = 123, Issue = "", PostCode = "412" };
                    break;
            }
            ccSample.Country = "GB";
            return ccSample;
        }

    }
}