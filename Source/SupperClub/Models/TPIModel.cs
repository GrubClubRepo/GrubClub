using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public class TPIModel
    {
        public IList<Event> EventList { get; set; }         
    }

    public class TPIBookingModel
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
        public List<TPIBookingMenuModel> bookingMenuModel { get; set; }
        public List<TPIBookingSeatingModel> bookingSeatingModel { get; set; }
        public decimal totalAfterDiscount
        {
            get
            {
                return (this.totalDue - this.discount);
            }
        }
    }

    public class TPIBookingMenuModel
    {
        public int menuOptionId { get; set; }
        public string menuTitle { get; set; }
        public int numberOfTickets { get; set; }
        public decimal baseTicketCost { get; set; }
        public decimal discount { get; set; }
    }
    public class TPIBookingSeatingModel
    {
        public int seatingId { get; set; }
        public DateTime start { get; set; }
        public int availableSeats { get; set; }
    }
    public class TPIChooseTicketsModel
    {
        public TPIBookingModel BookingModel { get; set; }
        public Event Event { get; set; }
        public int NumberOfTicketsAvailable { get; set; }
    }
}