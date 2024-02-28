using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataAnnotationsExtensions;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using SupperClub.Domain.Repository;

namespace SupperClub.Models
{
    public class BookingModel
    {
        public int eventId { get; set; }
        public string EventName { get; set; }
        public string EventDateAndTime { get; set; }
        public bool IsContactNumberRequired { get; set; }
        public string contactNumber { get; set; }
        public bool updateContactNumber { get; set; }
        public bool MinMaxBookingEnabled { get; set; }
        public int MinTicketsAllowed { get; set; }
        public int MaxTicketsAllowed { get; set; }
        public bool SeatSelectionInMultipleOfMin { get; set; }
        public bool ToggleMenuSelection { get; set; }

        [Required(ErrorMessage = "*")]
        [Integer(ErrorMessage = "You must enter a valid number of tickets")]
        [Range(1, 30, ErrorMessage = "You can buy 1 to 30 tickets only")]
        public int numberOfTickets { get; set; }

        [StringLength(1500, ErrorMessage = "1500 chars max")]
        public string bookingRequirements { get; set; }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal baseTicketCost { get; set; }
        public decimal commission { get; set; }
        public decimal totalTicketCost { get; set; }
        public decimal totalDue { get; set; }
        public decimal discount { get; set; }
        public string currency { get; set; }
        public int seatingId { get; set; }
        public int voucherId { get; set; }
        public string voucherDescription { get; set; }
        public List<BookingMenuModel> bookingMenuModel { get; set; }
        public List<BookingSeatingModel> bookingSeatingModel { get; set; }
        public decimal totalAfterDiscount
        {
            get
            {
                return (this.totalDue - this.discount);
            }
        }
    }
    public class BookingMenuModel
    {
        public int menuOptionId { get; set; }
        public string menuTitle { get; set; }
        public int numberOfTickets { get; set; }
        public decimal baseTicketCost { get; set; }
        public decimal discount { get; set; }
        //public bool minMaxBookingEnabled { get; set; }
        //public int minTicketsAllowed { get; set; }
        //public int maxTicketsAllowed { get; set; }
        //public bool seatSelectionInMultipleOfMin { get; set; }
    }
    public class BookingSeatingModel
    {
        public int seatingId { get; set; }
        public DateTime start { get; set; }
        //public bool minMaxBookingEnabled { get; set; }
        //public int minTicketsAllowed { get; set; }
        //public int maxTicketsAllowed { get; set; }
        //public bool seatSelectionInMultipleOfMin { get; set; }
    }
    public class ChooseTicketsModel
    {
        public BookingModel BookingModel { get; set; }
        public List<SelectListItem> NumberTicketsList { get; set; }
    }
    public class BookTicketsModel
    {
        public BookingModel BookingModel { get; set; }
        public Event Event { get; set; }
        public int NumberOfTicketsAvailable { get; set; }
    }
    public class TransactionResult
    {
        public bool Success { get; set; }
        public string TransactionMessage { get; set; }
        public int EventId { get; set; }
    }
    public class CreditCardDetailsViewModelOld
    {
        public List<SelectListItem> CreditCardTypes { get; set; }
        //public string paymentNonce { get; set; }
        public BookingModel bookingModel { get; set; }
        public CreditCardDetailsModel creditCardDetails { get; set; }
    }
    public class BraintreeCreditCard
    {
        public string CardType { get; set; } // VISA, MC, DELTA, MAESTRO, UKE, AMEX, DC, JCB, LASER, PAYPAL
        public string CardLastFour { get; set; }
        public string ExpirationMonth { get; set; }
        public string ExpirationYear { get; set; }
        public bool IsDefault { get; set; }
        public string Token { get; set; }
    }
    public class CreditCardDetailsViewModel
    {
        public List<SelectListItem> CreditCardTypes { get; set; }
        public BookingModel bookingModel { get; set; }
        public CreditCardDetailsModel creditCardDetails { get; set; }
        public Event Event { get; set; }
        public string ContactNumber { get; set; }       

    }
    public class CreditCardDetailsBraintreeViewModel
    {
        public BookingModel bookingModel { get; set; }
        public CreditCardDetailsBraintreeModel creditCardDetails { get; set; }
        public Event Event { get; set; }
        public string ContactNumber { get; set; }
        public string PaymentNonce { get; set; }
        public bool UseSavedCard { get; set; }
        public string SelectedToken { get; set; }
        public List<BraintreeCreditCard> SavedCardList { get; set; }
    }
    public class CreditCardDetailsModel
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "Card Type")]
        public string CardType { get; set; } // VISA, MC, DELTA, MAESTRO, UKE, AMEX, DC, JCB, LASER, PAYPAL
        [Required(ErrorMessage = "*Card number is required!")]
        [Display(Name = "Card Number")]
        [DataAnnotationsExtensions.CreditCard(ErrorMessage = "This is not a valid Credit Card Number")]
        public string CreditCardNumber { get; set; }
        [Required(ErrorMessage = "*Card Holder name is required!")]
        [Display(Name = "Name on the Card")]
        public string CardHolder { get; set; }

        // Optional (based on card type = MAESTRO)
        [Display(Name = "Start Date (MM/YY)")]
        [Digits]
        [Range(01, 12, ErrorMessage = "Start Month not valid")]
        public int? StartMonth { get; set; }
        [Digits]
        public int? StartYear { get; set; }
        [Display(Name = "Issue Number")]
        public int? IssueNumber { get; set; }

        [Required(ErrorMessage = "*Expiry month is required!")]
        [Display(Name = "Expiry Date (MM/YY)")]
        [Digits]
        [Range(1, 12, ErrorMessage = "Expiry Month not valid")]
        public int? ExpiryMonth { get; set; }

        [Required(ErrorMessage = "*Expiry year is required!")]
        [Digits]
        [Range(0, 99, ErrorMessage = "Expiry Year not valid")]
        public int? ExpiryYear { get; set; }

        [Required(ErrorMessage = "*CV2 is required!")]
        [Digits]
        [Range(0, 999, ErrorMessage = "CV2 should be a 3 digit number")]
        [DataType(DataType.Password)]
        [Display(Name = "CV2 Number")]
        public int? CV2 { get; set; }

        [Required(ErrorMessage = "*Address is required!")]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; } // Optional
        [Required(ErrorMessage = "*City is required!")]
        [Display(Name = "City")]
        public string City { get; set; }
        [Required(ErrorMessage = "*Post code is required!")]
        [Display(Name = "Post Code")]
        public string PostCode { get; set; }
        [Required(ErrorMessage = "*Country is required!")]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
    public class CreditCardDetailsBraintreeModel
    {
        [Required(ErrorMessage = "*Card number is required!")]
        [Display(Name = "Card Number")]
        [DataAnnotationsExtensions.CreditCard(ErrorMessage = "This is not a valid Credit Card Number")]
        public string CreditCardNumber { get; set; }
        [Required(ErrorMessage = "*Card Holder name is required!")]
        [Display(Name = "Name on the Card")]
        public string CardHolder { get; set; }

        // Optional (based on card type = MAESTRO)
        [Display(Name = "Start Date (MM/YY)")]
        [Digits]
        [Range(01, 12, ErrorMessage = "Start Month not valid")]
        public int? StartMonth { get; set; }
        [Digits]
        public int? StartYear { get; set; }
        [Display(Name = "Issue Number")]
        public int? IssueNumber { get; set; }

        [Required(ErrorMessage = "*Expiry month is required!")]
        [Display(Name = "Expiry Date (MM/YY)")]
        [Digits]
        [Range(1, 12, ErrorMessage = "Expiry Month not valid")]
        public int? ExpiryMonth { get; set; }

        [Required(ErrorMessage = "*Expiry year is required!")]
        [Digits]
        [Range(0, 99, ErrorMessage = "Expiry Year not valid")]
        public int? ExpiryYear { get; set; }

        [Required(ErrorMessage = "*CV2 is required!")]
        [Digits]
        [Range(0, 9999, ErrorMessage = "CV2 should be a 3 or 4 digit number")]
        [DataType(DataType.Password)]
        [Display(Name = "CV2 Number")]
        public int? CV2 { get; set; }

        [Required(ErrorMessage = "*Address is required!")]
        [Display(Name = "Address Line 1")]
        public string Address1 { get; set; }
        [Display(Name = "Address Line 2")]
        public string Address2 { get; set; } // Optional
        [Required(ErrorMessage = "*City is required!")]
        [Display(Name = "City")]
        public string City { get; set; }
        [Required(ErrorMessage = "*Post code is required!")]
        [Display(Name = "Post Code")]
        public string PostCode { get; set; }
        [Required(ErrorMessage = "*Country is required!")]
        [Display(Name = "Country")]
        public string Country { get; set; }
    }
    public enum CreditCardType
    {
        NONE = 0,
        VISA = 1,
        MC = 2,
        DELTA = 3,
        UKMAESTRO = 4,
        INTMAESTRO = 5,
        AMEX = 6,
        UKE = 7,
        JCB = 8,
        DINERS = 9,
        LASER = 10,
    }
    public class APIBookingModel
    {
        public int EventId { get; set; }
        public Guid UserId { get; set; }
        public int SeatingId { get; set; }
        public int MenuOptionId { get; set; }
        public int VoucherId { get; set; }
        public int NumberOfTickets { get; set; }
        public decimal BasePrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public string MenuTitle { get; set; }
        public Guid BasketId { get; set; }
        public decimal commission { get; set; }

    }
    public class BookingSuccessModel
    {
        public string supperClubName { get; set; }

        public Event BookingEvent { get; set; }

        public BookingModel bookingModel { get; set; }


    }
    public class BookingConfirmModel
    {
        public ChooseTicketsModel ChooseTicketModel
        {
            get;
            set;
        }
        public CreditCardDetailsViewModel CreditCardDetailsViewModel
        {
            get;
            set;
        }

    }
}