using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFirstStoredProcs;
using System.Web.Configuration;
using System.Web.Mvc;

namespace SupperClub.Domain
{
    public class EventListing   
    {
        [StoredProcAttributes.Name("StartDate")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.DateTime)]
        public DateTime StartDate { get; set; }

        [StoredProcAttributes.Name("Distance")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int Distance { get; set; }      

        [StoredProcAttributes.Name("Latitude")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Float)]
        public double Latitude { get; set; }

        [StoredProcAttributes.Name("Longitude")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Float)]
        public double Longitude { get; set; }

        [StoredProcAttributes.Name("MinPrice")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Money)]
        public decimal MinPrice { get; set; }

        [StoredProcAttributes.Name("MaxPrice")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Money)]
        public decimal MaxPrice { get; set; }

        [StoredProcAttributes.Name("PageIndex")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int PageIndex { get; set; }

        [StoredProcAttributes.Name("ResultsPerPage")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int ResultsPerPage { get; set; }

        [StoredProcAttributes.Name("MaxSeatsAvailable")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int? MaxSeatsAvailable { get; set; }

        [StoredProcAttributes.Name("Charity")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Bit)]
        public bool? Charity { get; set; }

        [StoredProcAttributes.Name("Alcohol")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Bit)]
        public bool? Alcohol { get; set; }

        [StoredProcAttributes.Name("Offset")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int Offset { get; set; }

        [StoredProcAttributes.Name("UserId")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.UniqueIdentifier)]
        public Guid? UserId { get; set; }
        //[StoredProcAttributes.Name("QueryTag")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        //public string QueryTag { get; set; }
    }

    public class EventListingFilter
    {
        public List<int> DietIds {get; set;}
        public List<int> PriceRangeIds { get; set; }
        public List<int> CuisineIds { get; set; }
    }

    public class EventListingWithMetaData
    {
        public IList<EventListingResult> EventListingResults { get; set; }
        public int ResultCount { get; set; }
        public GeoLocation SearchLocation { get; set; }
        public GeoLocation Boundry { get; set; }
    }

    public class EventListingResult
    {
        [StoredProcAttributes.Name("RowNum")]
        public Int64 RowNum { get; set; }
        [StoredProcAttributes.Name("EventId")]
        public int EventId { get; set; }
        [StoredProcAttributes.Name("EventName")]
        public string EventName { get; set; }
        [StoredProcAttributes.Name("EventDescription")]
        public string EventDescription { get; set; }
        [StoredProcAttributes.Name("EventShortDescription")]
        public string EventShortDescription { get; set; }
        [StoredProcAttributes.Name("EventImage")]
        public string EventImage { get; set; }        
        
        [StoredProcAttributes.Name("EventStart")]
        private DateTime eventStart = new DateTime();
        public DateTime EventStart  { 
            get
            {
                return DateConverter.GetUTCTime(eventStart);
            }
            set
            {
                eventStart = value;
            }
        }
        [StoredProcAttributes.Name("EventEnd")]
        private DateTime eventEnd = new DateTime();
        public DateTime EventEnd  { 
            get
            {
                return DateConverter.GetUTCTime(eventEnd);
            }
            set
            {
                eventEnd = value;
            }
        }

        [StoredProcAttributes.Name("Commisssion")]
        public decimal Commisssion { get; set; }
        

        [StoredProcAttributes.Name("Cost")]
        private string cost;
        public string Cost { 
            get
            {
                return CostCalculator.CostToGuest(decimal.Parse(cost), this.Commisssion).ToString();
            }
            set
            {
                cost = value;
            }
        }
        [StoredProcAttributes.Name("Currency")]
        private string currency;
        public string Currency {
            get
            {
                return "£";
            }
            set
            {
                currency = value;
            }
        }
        [StoredProcAttributes.Name("TotalSeats")]
        public int TotalSeats { get; set; }
        [StoredProcAttributes.Name("GuestsAttending")]
        public int GuestsAttending { get; set; }
        
        [StoredProcAttributes.Name("latitude")]
        public double EventLatitude { get; set; }
        [StoredProcAttributes.Name("longitude")]
        public double EventLongitude { get; set; }
        [StoredProcAttributes.Name("AddressCity")]
        public string EventCity { get; set; }
        [StoredProcAttributes.Name("AddressPostCode")]
        public string EventPostCode { get; set; }
        [StoredProcAttributes.Name("EventUrlFriendlyName")]
        public string EventUrlFriendlyName { get; set; }

        [StoredProcAttributes.Name("MultiSeating")]
        public bool MultiSeating { get; set; }
                
        [StoredProcAttributes.Name("Distance")]
        public double Distance { get; set; }
        
        [StoredProcAttributes.Name("Charity")]
        public bool Charity { get; set; }

        [StoredProcAttributes.Name("Alcohol")]
        public bool Alcohol { get; set; }

        [StoredProcAttributes.Name("DietId")]
        public int  DietId { get; set; }

        [StoredProcAttributes.Name("IsFavourite")]
        //private bool isFavourite;
        public bool IsFavourite { get; set; }
        //    get
        //    {
        //        return (isFavourite == 1 ? 1 : 0);
        //    }
        //    set
        //    {
        //        isFavourite = value;
        //    }
        //}

        [StoredProcAttributes.Name("CuisineId")]
        public int CuisineId { get; set; }
        
        public string EventURL = "";        
    }

    public class MyBookedEventsWithMetaData
    {
        public IList<MyBookedEvent> PastEvents { get; set; }
        public IList<MyBookedEvent> FutureEvents { get; set; }
    }
    public class MyBookedEvent
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventImage { get; set; }
        public string EventURL { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public int BookingReference { get; set; }
        public int NumberOfTickets { get; set; }
        public int EventSeatingId { get; set; }
        //public decimal TicketPrice { get; set; }

        public virtual decimal Commision
        {
            get;
            set;
        }

        public decimal Cost
        {
            get
            {
                return CostCalculator.CostToGuest(BaseCost,Commision);                
            }
        }
        //private decimal ticketPrice;
        public decimal BaseCost { get; set; }
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

        public double EventLatitude { get; set; }
        public double EventLongitude { get; set; }
        public string EventAddress { get; set; }
        public string EventCity { get; set; }
        public string EventPostCode { get; set; }
        public string GuestName { get; set; }
        public decimal TotalTicketPrice
        {
            get
            {
                return CostCalculator.CostToGuest(BaseCost,Commision) * this.NumberOfTickets;                
            }
        }
        public decimal? EventRating { get; set; }
    }

    public class MyWaitlistedEventsWithMetaData
    {
        public IList<MyWaitlistedEvent> WaitListedEvents { get; set; }
    }
    public class MyFavouriteEventsWithMetaData
    {
        public IList<MyWaitlistedEvent> FavouriteEvents { get; set; }
    }
    public class MyWaitlistedEvent
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventImage { get; set; }
        public string EventURL { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        public decimal Cost { get; set; }
        public decimal BaseCost { get; set; }
        public string EventShortDescription { get; set; }
        public string EventDescription { get; set; }
        public int GuestsAttending { get; set; }
        public int TotalSeats { get; set; }
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

        public double EventLatitude { get; set; }
        public double EventLongitude { get; set; }
        public string EventAddress { get; set; }
        public string EventCity { get; set; }
        public string EventPostCode { get; set; }
    }
    
    public class MyBasketDetails
    {
        public int EventId { get; set; }
        public int BookingReference { get; set; }
        public int EventSeatingId { get; set; }
        //public decimal BasePrice { get; set; }
        //public Guid BasketId { get; set; }
    }

    public class EventList
    {
        [StoredProcAttributes.Name("PostCode")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string PostCode { get; set; }
    }
    public class EventListResult
    {
        [StoredProcAttributes.Name("EventId")]
        public int EventId { get; set; }
        [StoredProcAttributes.Name("EventName")]
        public string EventName { get; set; }
        [StoredProcAttributes.Name("EventDescription")]
        public string EventDescription { get; set; }
        [StoredProcAttributes.Name("EventImage")]
        public string EventImage { get; set; }
        [StoredProcAttributes.Name("EventStart")]
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
        [StoredProcAttributes.Name("EventEnd")]
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
        
        [StoredProcAttributes.Name("Cost")]
        public string Cost { get; set; }

        [StoredProcAttributes.Name("Currency")]
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
        
        [StoredProcAttributes.Name("EventPostCode")]
        public string EventPostCode { get; set; }
        [StoredProcAttributes.Name("EventUrlFriendlyName")]
        public string EventUrlFriendlyName { get; set; }
        [StoredProcAttributes.Name("Soldout")]
        public bool Soldout { get; set; }
        [StoredProcAttributes.Name("GrubClubName")]
        public string Chef { get; set; }
        [StoredProcAttributes.Name("GrubClubUrlFriendlyName")]
        public string GrubClubUrlFriendlyName { get; set; }
        [StoredProcAttributes.Name("SupperClubId")]
        public int SupperClubId { get; set; }
        public string EventURL = "";
        public string ChefProfileURL = "";
    }

    public class EventListOutput
    {
        public string EventName { get; set; }
        public string EventDescription { get; set; }
        public string EventImage { get; set; }
        public DateTime EventStart { get; set; }
        public DateTime EventEnd { get; set; }
        
        public string Cost { get; set; }
        public string Currency { get; set; }

        public string EventPostCode { get; set; }
        public bool Soldout { get; set; }
        public string Chef { get; set; }
        public string EventURL = "";
        public string ChefProfileURL = "";
    }
}
