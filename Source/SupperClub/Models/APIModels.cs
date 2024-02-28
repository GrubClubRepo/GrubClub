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
    public class APIModel
    {
        public int eventId { get; set; }
        public string EventName { get; set; }
        public string EventDateAndTime { get; set; }
        public bool IsContactNumberRequired { get; set; }
        public string contactNumber { get; set; }
        public bool updateContactNumber { get; set; }
        
        [Required(ErrorMessage="*")]
        [Integer(ErrorMessage="You must enter a valid number of tickets")]
        [Range(1,30, ErrorMessage="You can buy 1 to 30 tickets only")]
        public int numberOfTickets { get; set; }
        
        [StringLength(1500, ErrorMessage = "1500 chars max")]
        public string bookingRequirements { get; set; }
        
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal baseTicketCost { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal totalTicketCost { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal totalDue { get; set; }
        public string currency { get; set; }
        public int seatingId { get; set; }
        public List<BookingMenuModel> bookingMenuModel { get; set; }
        public List<BookingSeatingModel> bookingSeatingModel { get; set; }
    }

    public class BookingParameterModel
    {
        public int menuOptionId { get; set; }        
        public int numberOfTickets { get; set; }
        public decimal baseTicketCost { get; set; }
        public decimal discount { get; set; }
    }
   
    public class AddTicketsStatusModel
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NumberOfTicketsAdded { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAfterDiscount {get; set;}
    }

    public class StatusModel
    {
        public bool Success { get; set; }
        public bool ValidVenmoSDK { get; set; }
        public string Message { get; set; }
        public int BookingReferenceNumber { get; set; }
        public bool GuestEmailSentStatus { get; set; }
    }
    public class EventDetailsWithMetaData
    {
        public IList<EventDetails> EventDetails { get; set; }
    }
    public class EventDetails
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public List<string> EventImage { get; set; }
        public string EventURL { get; set; }

        public bool EventMinMaxBookingEnabled { get; set; }
        public bool EventSeatSelectionInMultipleOfMinEnabled { get; set; }
        public bool EventToggleMenuSelectionEnabled { get; set; }
        public int EventMinTicketsAllowed { get; set; }
        public int EventMaxTicketsAllowed { get; set; } 

        private DateTime eventStart = new DateTime();
        public DateTime EventStart
        {
            get
            {
                return DateConverter.GetUTCTime(eventStart);
            }
            set
            {
                eventStart = value;
            }
        }
        private DateTime eventEnd = new DateTime();
        public DateTime EventEnd
        {
            get
            {
                return DateConverter.GetUTCTime(eventEnd);
            }
            set
            {
                eventEnd = value;
            }
        }
        public string EventDescription;
        public decimal BaseCost;
        public decimal Cost;
        public string EventAddress;
        public string EventCity;
        public string EventPostCode;
        public double EventLatitude;
        public double EventLongitude;
        public int GrubClubId;
        public string GrubClubName;
        public string GrubClubDescription;
        public List<string> GrubClubImage;
        public string GrubClubURL;
        public int TotalSeats;
        public int AvailableSeats;
        public bool ContactNumberRequired;
        public bool MultiSeating;
        public IList<MultiSeatingDetails> EventSeatingDetails;
        public bool MultiMenu;
        public IList<MultiMenuDetails> EventMenuDetails;
        public bool Alcohol;
        public bool Charity;
        public string EventMenu;
        public string EventDiet;
        public string EventCuisine;
        public int NumberOfVotes;
        public decimal? AverageRating;
        public IList<EventReview> EventReviews;
        private DateTime minSeatingTime = new DateTime();
        public DateTime MinSeatingTime
        {
            get
            {
                return DateConverter.GetUTCTime(minSeatingTime);
            }
            set
            {
                minSeatingTime = value;
            }
        }
        public bool UserAddedToWaitList;
        public IList<Guest> EventGuestList;
        private string currency;
        public string Currency
        {
            get
            {
                return "£";
            }
            set
            {
                currency = value;
            }
        }
    }
    public class MultiSeatingDetails
    {
        public int SeatingId;
        private DateTime eventStart = new DateTime();
        public DateTime SeatingStartTime
        {
            get
            {
                return DateConverter.GetUTCTime(eventStart);
            }
            set
            {
                eventStart = value;
            }
        }
        private DateTime eventEnd = new DateTime();
        public DateTime SeatingEndTime
        {
            get
            {
                return DateConverter.GetUTCTime(eventEnd);
            }
            set
            {
                eventEnd = value;
            }
        }
        public int SeatingGuests;
        public int SeatingAvailableSeats;

        public MultiSeatingDetails() { }
        
    }
    public class MultiMenuDetails
    {
        public int MenuOptionId;
        public string MenuTitle;
        public string MenuDescription;
        public decimal MenuOptionCost;

        public MultiMenuDetails()
        {
        }        
    }
    public class Guest
    {
        public string BookingReference;
        public string GuestUserId;
        public string GuestName;
        public string GuestEmail;
        public int EventSeatingId;
        public int EventMenuOptionId;
        public int NumberOfTickets;
        public string SpecialRequirements;
        public bool CheckInStatus;
        private DateTime? checkInDate;
        public DateTime? CheckInDate
        {
            get
            {
                if (checkInDate != null)
                    return DateConverter.GetUTCTime((DateTime)checkInDate);
                else
                    return checkInDate;
            }
            set
            {
                checkInDate = value;
            }
        }
    }

    public class EventReview
    {
        public int ReviewId { get; set; }
        public string Title { get; set; }
        public string PublicReview { get; set; }
        public bool Anonymous { get; set; }
        public int? Rating { get; set; }
        private DateTime reviewCreateDate;
        public DateTime ReviewCreateDate
        {
            get
            {
                if (reviewCreateDate != null)
                    return DateConverter.GetUTCTime((DateTime)reviewCreateDate);
                else
                    return reviewCreateDate;
            }
            set
            {
                reviewCreateDate = value;
            }
        }
        public string GuestName { get; set; }
        public string HostResponse { get; set; }
        public string AdminResponse { get; set; }
        private DateTime? adminResponseDate;
        public DateTime? AdminResponseDate
        {
            get
            {
                if (adminResponseDate != null)
                    return DateConverter.GetUTCTime((DateTime)adminResponseDate);
                else
                    return adminResponseDate;
            }
            set
            {
                adminResponseDate = value;
            }
        }
        private DateTime? hostResponseDate;
        public DateTime? HostResponseDate
        {
            get
            {
                if (hostResponseDate != null)
                    return DateConverter.GetUTCTime((DateTime)hostResponseDate);
                else
                    return hostResponseDate;
            }
            set
            {
                hostResponseDate = value;
            }
        }
    }

    public class NotificationStatesWithMetaData
    {
        public IList<NotificationState> Notifications { get; set; }
    }
    public class NotificationState
    {
        public string NotificationType { get; set; }
        public bool State { get; set; }
    }
}