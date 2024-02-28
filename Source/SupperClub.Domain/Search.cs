using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFirstStoredProcs;
using System.Web.Configuration;

namespace SupperClub.Domain
{
    public class Search   
    {
        //[StoredProcAttributes.Name("StartDate")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.DateTime)]
        //public DateTime StartDate { get; set; }

        [StoredProcAttributes.Name("Distance")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int Distance { get; set; }
      

        [StoredProcAttributes.Name("Latitude")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Float)]
        public double Latitude { get; set; }


        [StoredProcAttributes.Name("Longitude")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Float)]
        public double Longitude { get; set; }

        [StoredProcAttributes.Name("Guests")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        public int Guests { get; set; }

        //[StoredProcAttributes.Name("MinPrice")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.Money)]
        //public decimal MinPrice { get; set; }

        //[StoredProcAttributes.Name("MaxPrice")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.Money)]
        //public decimal MaxPrice { get; set; }

        [StoredProcAttributes.Name("FoodKeyword")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string FoodKeyword { get; set; }

        [StoredProcAttributes.Name("Diet")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string Diet { get; set; }

        //[StoredProcAttributes.Name("Charity")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.Bit)]
        //public bool? Charity { get; set; }

        //[StoredProcAttributes.Name("Alcohol")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.Bit)]
        //public bool? Alcohol { get; set; }

        //[StoredProcAttributes.Name("EndDateOffset")]
        //[StoredProcAttributes.ParameterType(System.Data.SqlDbType.Int)]
        //public int EndDateOffset { get; set; }

        [StoredProcAttributes.Name("QueryTag")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string QueryTag { get; set; }

        [StoredProcAttributes.Name("QueryCity")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string QueryCity { get; set; }

        [StoredProcAttributes.Name("QueryArea")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string QueryArea { get; set; }

        [StoredProcAttributes.Name("UserId")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.UniqueIdentifier)]
        public Guid? UserId { get; set; }
    }

    public class SearchFilter
    {
        public List<int> DietIds {get; set;}
        public List<int> PriceRangeIds { get; set; }
        public List<int> CuisineIds { get; set; }
        public List<int> TagIds { get; set; }
    }

    public class SearchResultWithMetaData
    {
        public IList<SearchResult> SearchResults { get; set; }
        public int SearchResultCount { get; set; }
        public GeoLocation SearchLocation { get; set; }
        public GeoLocation Boundry { get; set; }
    }

    public class SearchResult
    {
        [StoredProcAttributes.Name("EventId")]
        public int EventId { get; set; }
        [StoredProcAttributes.Name("EventName")]
        public string EventName { get; set; }
        [StoredProcAttributes.Name("EventDescription")]
        public string EventDescription { get; set; }
        [StoredProcAttributes.Name("EventImage")]
        public string EventImage { get; set; }

        [StoredProcAttributes.Name("EventDateTime")]
        public DateTime EventDateTime { get; set; }
        public string EventDate
        {
            get
            {
                return EventDateTime.ToString("ddd, d MMM");
            }
            set
            {
                cost = value;
            }
        }
        
        [StoredProcAttributes.Name("EventStart")]
        public string EventStart { get; set; }
        [StoredProcAttributes.Name("EventEnd")]
        public string EventEnd { get; set; }
        [StoredProcAttributes.Name("Byob")]
        public bool Byob { get; set; }
        [StoredProcAttributes.Name("Charity")]
        public bool Charity { get; set; }
        public virtual decimal Commision
        {
            get;
            set;
        }

       
        [StoredProcAttributes.Name("Cost")]
        private string cost;

        public string Cost
        {
            get
            {
                return string.Format("{0:C}", CostCalculator.CostToGuest(decimal.Parse(cost), this.Commision));
            }
            set
            {
                cost = value;
            }
        }
        private decimal costToGuest;
        public decimal CostToGuest
        {
            get
            {
                return CostCalculator.CostToGuest(decimal.Parse(cost), this.Commision);
            }
            set { costToGuest = value; }
        }
        [StoredProcAttributes.Name("TotalSeats")]
        public int TotalSeats { get; set; }
        [StoredProcAttributes.Name("GuestsAttending")]
        public int GuestsAttending { get; set; }
        [StoredProcAttributes.Name("TicketsSold")]
        public int TicketsSold { get; set; }

        [StoredProcAttributes.Name("lat")]
        public double lat { get; set; }
        [StoredProcAttributes.Name("lng")]
        public double lng { get; set; }
        [StoredProcAttributes.Name("EventPostCode")]
        private string eventPostCode;

        public string EventPostCode
        {
            get
            {
                return eventPostCode.Split(new string[] { " " }, StringSplitOptions.None)[0];
            }
            set
            {
                eventPostCode = value;
            }
        }
        
        [StoredProcAttributes.Name("EventUrlFriendlyName")]
        public string EventUrlFriendlyName { get; set; }

        [StoredProcAttributes.Name("MultiSeating")]
        public bool MultiSeating { get; set; }

        [StoredProcAttributes.Name("GrubClubName")]
        public string GrubClubName { get; set; }
        [StoredProcAttributes.Name("GrubClubUrlFriendlyName")]
        public string GrubClubUrlFriendlyName { get; set; }
        //[StoredProcAttributes.Name("Address")]
        //public string Address { get; set; }
        //[StoredProcAttributes.Name("Address2")]
        //public string Address2 { get; set; }
        //[StoredProcAttributes.Name("City")]
        //public string City { get; set; }
        //[StoredProcAttributes.Name("Country")]
        //public string Country { get; set; }
        //[StoredProcAttributes.Name("SupperClubName")]
        //public string SupperClubName { get; set; }
        //[StoredProcAttributes.Name("SupperClubTradingName")]
        //public string SupperClubTradingName { get; set; }
        //[StoredProcAttributes.Name("Summary")]
        //public string Summary { get; set; }


        [StoredProcAttributes.Name("Distance")]
        public double Distance { get; set; }

        [StoredProcAttributes.Name("DietId")]
        public int DietId { get; set; }

        [StoredProcAttributes.Name("CuisineId")]
        public int CuisineId { get; set; }

        [StoredProcAttributes.Name("TagId")]
        public int TagId { get; set; }

        [StoredProcAttributes.Name("SupperclubId")]
        public int SupperclubId { get; set; }


        [StoredProcAttributes.Name("BrandNew")]
        public string BrandNew { get; set; }

        [StoredProcAttributes.Name("ReviewCount")]
        public int ReviewCount { get; set; }

        [StoredProcAttributes.Name("Rating")]
        private decimal rating;
        public Decimal Rating
        {
            get
            {
                return Math.Round(rating, MidpointRounding.AwayFromZero);
            }
            set
            {
                rating = value;
            }
        }
        
        [StoredProcAttributes.Name("EarlyBird")]
        public int? EarlyBird { get; set; }

        // private string earlyBirdPrice;
        [StoredProcAttributes.Name("EarlyBirdPrice")]
        public string EarlyBirdPrice
        {

            get;
            set;
            //{
            //    return string.Format("{0:C}", CostCalculator.CostToGuest(decimal.Parse(earlyBirdPrice)));
            //}
            //set
            //{
            //    earlyBirdPrice = value;
            //}

        }

        [StoredProcAttributes.Name("WishEvent")]
        public int WishEvent { get; set; }

        [StoredProcAttributes.Name("EventCount")]
        public int EventCount { get; set; }

        [StoredProcAttributes.Name("SeatsAvailable")]
        public bool SeatsAvailable { get; set; }

        public string ListingTitle { get; set; }
        private bool _showListingTitle = false;
        public bool ShowListingTitle
        {
            get
            {
                return _showListingTitle;
            }
            set
            {
                _showListingTitle = value;
            }

        }
    }
}
