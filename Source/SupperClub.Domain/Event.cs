using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using System.Web.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Text.RegularExpressions;
using CodeFirstStoredProcs;

namespace SupperClub.Domain
{
    public enum EventStatus
    {
        Active = 1,
        Cancelled = 2,
        New = 3,
        Rejected = 4
    }
    public class Event : IValidatableObject
    {

        #region Primitive Properties

        #region Database Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Event Title Required")]
        [Display(Name = "Event Title *")]
        public virtual string Name
        {
            get;
            set;
        }

        
        [Required(ErrorMessage = "Description Required")]
        [Display(Name = "Description *")]
        [DataType(DataType.MultilineText)]
      //  [AllowHtml]
        public virtual string Description
        {
            get;
            set;
        }

        [DataType(DataType.MultilineText)]
        public virtual string EventDescription
        {
            get
            {
                return this.Description.Replace("|&|", System.Environment.NewLine);
            }         
        }        

        [Required(ErrorMessage = "Date of Event Required")]
        [Display(Name = "Date of Event *")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public virtual System.DateTime Start
        {
            get;
            set;
        }

        [Display(Name = "Choose Seating Type")]
        public virtual bool MultiSeating
        {
            get;
            set;
        }

        [Display(Name = "Do you have multiple course options to choose from?")]
        public virtual bool MultiMenuOption
        {
            get;
            set;
        }

        [Display(Name = "Does this event has a minimum number of booking requirement?")]
        public virtual bool MinMaxBookingEnabled
        {
            get;
            set;
        }
        [Display(Name = "Could this event be booked only in multiple of minimum bookings allowed?")]
        public virtual bool SeatSelectionInMultipleOfMin
        {
            get;
            set;
        }
        [Display(Name = "Should the user be allowed only to select only one menu option?")]
        public virtual bool ToggleMenuSelection
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Minimum Tickets Allowed Required")]
        [Display(Name = "Minimum Tickets Allowed in a Single Booking")]
        public virtual int MinTicketsAllowed
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Maximum Tickets Allowed Required")]
        [Display(Name = "Maximum Tickets Allowed in a Single Booking")]
        public virtual int MaxTicketsAllowed
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Start Time Required")]
        [Display(Name = "Start Time *")]
        public virtual System.DateTime StartTime
        {
            get;
            set;
        }
       
        public virtual System.DateTime End
        {
            get;
            set;
        }
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Duration Required")]
        [Display(Name = "How many hours long is it? *")]
        [Min(1, ErrorMessage = "Please provide a valid duration")]
        public virtual double Duration
        {
            get
            {
                if (duration != 0)
                    return duration;
                else
                    return (End - Start).TotalHours;
            }
            set
            {
                duration = value;
            }
        }
        private double duration;

        public virtual int SupperClubId
        {
            get { return _supperClubId; }
            set
            {
                if (_supperClubId != value)
                {
                    if (SupperClub != null && SupperClub.Id != value)
                    {
                        SupperClub = null;
                    }
                    _supperClubId = value;
                }
            }
        }
        private int _supperClubId;



        [Required(ErrorMessage = "Address 1 Required")]
        [StringLength(500, ErrorMessage = "Address cannot be longer than 500 characters.")]
        [Display(Name = "Address 1 *")]
        public virtual string Address
        {
            get;
            set;
        }

        [StringLength(500, ErrorMessage = "Address2 cannot be longer than 500 characters.")]
        [Display(Name = "Address 2")]
        public virtual string Address2
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Town/City Required")]
        [StringLength(500, ErrorMessage = "City name cannot be longer than 500 characters.")]
        [Display(Name = "Town/City *")]
        public virtual string City
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Post Code Required")]
        [StringLength(10, ErrorMessage = "Post code cannot be longer than 10 characters.")]
        [Display(Name = "Post Code *")]
        public virtual string PostCode
        {
            get;
            set;
        }


        public virtual double Latitude
        {
            get;
            set;
        }


        public virtual double Longitude
        {
            get;
            set;
        }

        /// <summary>
        /// Number of seats available at the event.
        /// </summary>
        [Integer]
        [Display(Name = "Seats Available *")]
        [Min(1, ErrorMessage = "Please provide a valid number of seats")]
        [Required(ErrorMessage = "Seats Available is Required")]
        public virtual int Guests
        {
            get;
            set;
        }

        /// <summary>
        /// Number of reserved seats for the event (these should not be sold).
        /// </summary>
        [Integer(ErrorMessage="Must be a number")]
        [Display(Name = "Reserved Seats")]
        [Min(0, ErrorMessage = "Please provide a valid number of reserved seats")]
        public virtual int ReservedSeats
        {
            get;
            set;
        }

        [Required]
        [Display(Name = "Are guests permitted to bring their own alcohol?")]
        public virtual bool Alcohol
        {
            get;
            set;
        }

        [Required]
        [Display(Name = "Is this a charity event?")]
        public virtual bool Charity
        {
            get;
            set;
        }

        [Required]
        [Display(Name = "Is this a private event?")]
        public virtual bool Private
        {
            get;
            set;
        }
        public virtual Nullable<int> EarlyBird
        {
            get;
            set;
        }
        public virtual Nullable<decimal> EarlyBirdPrice
        {
            get;
            set;
        }
        [Required]
        [Display(Name = "Do you want to collect diners' contact numbers?")]
        public virtual bool ContactNumberRequired
        {
            get;
            set;
        }
        [Required]
        [Display(Name = "Could guests reach anytime between the event time window?")]
        public virtual bool WalkIn
        {
            get;
            set;
        }

        [Required]
        [Display(Name = "Commission Required")]
        public virtual decimal Commission
        {
            get;
            set;
        }
        //[Required]
        //[Display(Name = "Does this event has a minimum number of booking requirement?")]
        //public virtual bool MinGuestCountRequired
        //{
        //    get;
        //    set;
        //}
        //[Required]
        //[Display(Name = "Minimum number of bookings required for event")]
        //public virtual int MinGuestCount
        //{
        //    get;
        //    set;
        //}

        [Display(Name = "Cuisines")]
        public virtual string Cuisines
        {
            get
            {
                if (cuisines == "" || cuisines == null)
                {
                    cuisines = "";
                    foreach (EventCuisine ec in this.EventCuisines)
                        cuisines += ec.Cuisine.Name + ", ";
                    if (cuisines.Length > 0)
                        cuisines = cuisines.Substring(0, cuisines.Length - 2);
                }
                return cuisines;
            }
            set
            {
                cuisines = value;
            }
        }
        private string cuisines;

        public virtual List<int> SelectedTagIds
        {
            get
            {
                if (tagIds != null && tagIds.Count == 0 && this.EventTags != null)
                {
                    foreach (EventTag t in this.EventTags)
                        tagIds.Add(t.Tag.Id);                    
                }
                return tagIds;
            }
            set
            {
                tagIds = value;
            }
        }
        private List<int> tagIds = new List<int>();
        public virtual List<int> SelectedAreaIds
        {
            get
            {
                if (areaIds != null && areaIds.Count == 0 && this.EventAreas != null)
                {
                    foreach (EventArea t in this.EventAreas)
                        areaIds.Add(t.Area.Id);
                }
                return areaIds;
            }
            set
            {
                areaIds = value;
            }
        }
        private List<int> areaIds = new List<int>();
        public virtual List<int> SelectedCityIds
        {
            get
            {
                if (cityIds != null && cityIds.Count == 0 && this.EventCities != null)
                {
                    foreach (EventCity t in this.EventCities)
                        cityIds.Add(t.City.Id);
                }
                return cityIds;
            }
            set
            {
                cityIds = value;
            }
        }
        private List<int> cityIds = new List<int>();

        public virtual string OtherAllergyText
        {
            get
            {
                if (this.EventDiets != null && this.EventDiets.Count > 0)
                {
                    var diet = (from x in this.EventDiets where x.Diet.DietTypeId == (int)DietType.Others select x.Diet.Name).FirstOrDefault();
                    if (diet != null)
                    {
                        otherAllergyText = diet;
                    }
                }
                return otherAllergyText;
            }
            set
            {
                otherAllergyText = value;
            }
        }
        private string otherAllergyText = "";

        public virtual string EventTagList
        {
            get
            {
                if (eventTagList == null || eventTagList == "")
                {
                    eventTagList = "";
                    if (this.tagIds != null)
                    {
                        foreach (int et in this.tagIds)
                            eventTagList += et.ToString() + ",";
                        if (eventTagList.Length > 0)
                            eventTagList = eventTagList.Substring(0, eventTagList.Length - 1);
                    }
                }
                return eventTagList;
            }
            set
            {
                eventTagList = value;
            }
        }
        private string eventTagList;

        public virtual string EventCityList
        {
            get
            {
                if (eventCityList == null || eventCityList == "")
                {
                    eventCityList = "";
                    if (this.cityIds != null)
                    {
                        foreach (int ec in this.cityIds)
                            eventCityList += ec.ToString() + ",";
                        if (eventCityList.Length > 0)
                            eventCityList = eventCityList.Substring(0, eventCityList.Length - 1);
                    }
                }
                return eventCityList;
            }
            set
            {
                eventCityList = value;
            }
        }
        private string eventCityList;

        public virtual string EventAreaList
        {
            get
            {
                if (eventAreaList == null || eventAreaList == "")
                {
                    eventAreaList = "";
                    if (this.areaIds != null)
                    {
                        foreach (int ea in this.areaIds)
                            eventAreaList += ea.ToString() + ",";
                        if (eventAreaList.Length > 0)
                            eventAreaList = eventAreaList.Substring(0, eventAreaList.Length - 1);
                    }
                }
                return eventAreaList;
            }
            set
            {
                eventAreaList = value;
            }
        }
        private string eventAreaList;

        [Display(Name = "Seatings")]
        public virtual string Seatings
        {
            get
            {
                if (seatings == "" || seatings == null)
                {
                    seatings = "";
                    foreach (EventSeating es in this.EventSeatings)
                        seatings += es.Start.Hour.ToString() + ":" + es.Start.Minute.ToString() + "|" + es.Guests.ToString() + "|" + es.ReservedSeats.ToString() + "|" + (es.IsDefault ? "1" : "0") + "|" + es.Id.ToString() + ",";
                    if (seatings.Length > 0)
                        seatings = seatings.Substring(0, seatings.Length - 1);
                }
                return seatings;
            }
            set
            {
                seatings = value;
            }
        }
        private string seatings;

        [Display(Name = "MenuOptions")]
        public virtual string MenuOptions
        {
            get
            {
                if (menuOptions == null || menuOptions == "")
                {
                    menuOptions = "";
                    foreach (EventMenuOption emo in this.EventMenuOptions)
                        menuOptions += Microsoft.JScript.GlobalObject.escape(emo.Title) + "|" + Microsoft.JScript.GlobalObject.escape(emo.Description) + "|" + emo.Cost.ToString() + "|" + (emo.IsDefault ? "1" : "0") + "|" + emo.Id.ToString() + ",";
                    if (menuOptions.Length > 0)
                        menuOptions = menuOptions.Substring(0, menuOptions.Length - 1);
                }
                return menuOptions;
            }
            set
            {
                menuOptions = value;
            }
        }
        private string menuOptions;

        [Display(Name = "Specific Diet")]
        public virtual string Diets
        {
            get
            {
                if (diets == "" || diets == null)
                {
                    diets = "";
                    foreach (EventDiet ed in this.EventDiets)
                        diets += ed.Diet.Name + "|" + ed.Diet.DietTypeId.ToString() + "%";
                    if (diets.Length > 0)
                        diets = diets.Substring(0, diets.Length - 1);
                }
                return diets;
            }
            set
            {
                diets = value;
            }
        }
        private string diets;

        public virtual List<int> SelectedDietIds
        {
            get
            {
                if (dietIds != null && dietIds.Count == 0 && this.EventDiets != null)
                {
                    foreach (EventDiet d in this.EventDiets)
                        dietIds.Add(d.DietId);
                }
                return dietIds;
            }
            set
            {
                dietIds = value;
            }
        }
        private List<int> dietIds = new List<int>();

        [Display(Name = "What's on the menu")]
        [DataType(DataType.MultilineText)]
        //[AllowHtml]
        public virtual string Menu
        {
            get
            {
                if (menu == "" || menu == null)
                {
                    menu = "";
                    foreach (Menu m in this.Menus)
                        menu += m.MenuItem + System.Environment.NewLine;
                }
                return menu;
            }
            set
            {
                menu = value;
            }
        }
        private string menu;

        public virtual string ImagePaths
        {
            get
            {
                if (imagePaths == null || imagePaths == "")
                {
                    imagePaths = "";
                    foreach (EventImage ei in this.EventImages)
                        imagePaths += ei.ImagePath + ",";
                    if (imagePaths.Length > 0)
                        imagePaths = imagePaths.Substring(0, imagePaths.Length - 1);
                }
                return imagePaths;
            }
            set
            {
                imagePaths = value;
            }
        }
        private string imagePaths;

        public virtual string ImagePath
        {
            get;
            set;
        }

        [Display(Name = "House Rules")]
        [DataType(DataType.MultilineText)]
        public virtual string HouseRule
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Cost is Required")]
        [Display(Name = "How much does it cost per person? *", Prompt= "£10.00")]
        [DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = true) ]
        public virtual decimal Cost 
        {
            get;
            set;
        }

        public virtual DateTime DateCreated
        {
            get;
            set;
        }
        public virtual DateTime? DateApproved
        {
            get;
            set;
        }
        public virtual bool Active
        {
            get;
            set;
        }

        public virtual int Status
        {
            get;
            set;
        }

        
        
        public virtual bool HomeEvent
        {
            get;
            set;
        }

        [Display(Name = "Local file")]
        public HttpPostedFileBase File { get; set; }

        #endregion

        #region Calculated Values (Not in Db)
        public string NonHtmlDescription
        {
            get
            {
                return Regex.Replace(EventDescription, "<.+?>", string.Empty);
            }
        }
        public string ShortDescription
        {
            get
            {
                return HtmlContentFormatter.GetSubString(this.EventDescription, 250);
            }
        }
        public string NameDateTime
        {
            get
            {
                return string.Format("{0} - {1} - {2}", this.Name, this.Start.ToString("ddd, d MMM yyyy"), this.Start.ToShortTimeString());
            }
        }

        // [Required(ErrorMessage = "*")] // This breaks updating despite being set to ignore
       // [Display(Name = "How much does it cost per person? *", Prompt = "£X.00")]
         [Display(Name = "How much are you planning to receive per person? *", Prompt = "£X.00")]
         [DisplayFormat(DataFormatString = "{0:F4}", ApplyFormatInEditMode = true)]
        public string CostTextDisplay
        {
            get;
            set;
        }

        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal CostToGuest
        {
            get
            {
                return CostCalculator.CostToGuest(this.Cost, this.Commission);
            }
        }


        /// <summary>
        /// Gets number of seats booked by an attendee
        /// </summary>
        public int NumberOfAttendeeGuests(Guid? userId)
        {
            if (userId != null)
            {
                int i = 0;
                foreach (EventAttendee e in this.EventAttendees)
                {
                    if (e.UserId == userId)
                        i += e.NumberOfGuests;
                }
                return i;
            }
            return 0;
        }

        /// <summary>
        /// Gets total number of seats booked by all Attendees.
        /// </summary>
        public int TotalNumberOfAttendeeGuests
        {
            get
            {
                int i = 0;
                if(this.EventAttendees != null && this.EventAttendees.Count > 0)
                { 
                    foreach (EventAttendee e in this.EventAttendees)
                    {
                        i += e.NumberOfGuests;
                    }
                }
                return i;
            }
        }

        /// <summary>
        /// Gets total number of availble seats.
        /// </summary>
        public int TotalNumberOfAvailableSeats
        {
            get
            {
                if (!MultiSeating)
                    return (this.Guests - this.ReservedSeats - this.TotalNumberOfAttendeeGuests);
                else
                {
                    int rs=0; int totalGuests = 0;
                    foreach (EventSeating es in this.EventSeatings)
                    {
                        totalGuests += es.Guests;
                        rs += es.ReservedSeats;                        
                    }
                    return (totalGuests - rs - this.TotalNumberOfAttendeeGuests);
                }
            }
        }

        /// <summary>
        /// Gets total number of availble seats.
        /// </summary>
        public int TPIAvailableSeats
        {
            get;
            set;
        }
        public int TotalEventGuests
        {
            get
            {
                int i = 0;
                if (this.MultiSeating)
                {
                    foreach (EventSeating e in this.EventSeatings)
                    {
                        i += e.Guests;
                    }
                }
                else
                    i = this.Guests;
                return i;
            }
        }
        public int TotalEventReservedSeats
        {
            get
            {
                int i = 0;
                if (this.MultiSeating)
                {
                    foreach (EventSeating e in this.EventSeatings)
                    {
                        i += e.ReservedSeats;
                    }
                }
                return i;
            }
        }
        
        /// <summary>
        /// Gets average rank from all reviewers that provided a ranking.
        /// Returns null if no ranking data available.
        /// </summary>
        public decimal AverageRank
        {
            get
            {
                decimal averageRank = 0;
                decimal totalScore = 0;
                int numberGuestsRanked = 0;

                if (this.Reviews != null)
                {
                    foreach (Review r in this.Reviews)
                    {
                        if (r.Rating != null)
                        {
                            totalScore += (decimal)r.Rating;
                            numberGuestsRanked++;
                        }
                    }
                }

                if (numberGuestsRanked > 0)
                {
                    averageRank =  Math.Round((totalScore / (decimal)numberGuestsRanked), MidpointRounding.AwayFromZero);  
                }

                return averageRank;
            }
        }

        /// <summary>
        /// Gets total number of all reviewers that provided a ranking.
        /// </summary>
        public int NumberGuestRanks
        {
            get
            {
                int numberGuestRanks = 0;
                if (this.Reviews != null)
                {
                    foreach (Review r in this.Reviews)
                    {
                        if (r.Rating != null)
                        {
                            numberGuestRanks++;
                        }
                    }
                }
                return numberGuestRanks;
            }
        }

        public int DefaultSeatingId
        {
            get
            {
                foreach (EventSeating es in this.EventSeatings)
                {
                    if (es.IsDefault)
                    {
                        _defaultSeatingId = es.Id;
                        break;
                    }
                }
                return _defaultSeatingId;
            }
            set
            {
                this._defaultSeatingId = value;
            }
        }
        private int _defaultSeatingId = 0;
        public int DefaultMenuOptionId
        {
            get
            {
                foreach (EventMenuOption emo in this.EventMenuOptions)
                {
                    if (emo.IsDefault)
                    {
                        _defaultMenuOptionId = emo.Id;
                        break;
                    }
                }
                return _defaultMenuOptionId;
            }
            set
            {
                _defaultMenuOptionId = value;
            }
        }
        private int _defaultMenuOptionId = 0;
        public DateTime MinSeatingTime
        {           
            get
            {
                DateTime minTime = this.Start;
                if (this.MultiSeating && this.EventSeatings != null && this.EventSeatings.Count > 0)
                    minTime = this.EventSeatings.OrderBy(x => x.Start).First().Start;
                return minTime;
            }
        }
        #endregion

        #endregion

        #region Validation Business Rules

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Guests < 1)
                yield return new ValidationResult("The number of guests must be 1 or more", new[] { "Guests" });

            if (this.Id == 0 && Start < DateTime.Now) // Only apply this validation to new Events (Updates are exempt)
                yield return new ValidationResult("Starting date and time must be in the future", new[] { "Start" });

            if (TotalNumberOfAttendeeGuests > Guests)
                yield return new ValidationResult("This event has already sold " + TotalNumberOfAttendeeGuests.ToString() + " seats", new[] { "Guests" });

            if (ReservedSeats > Guests)
                yield return new ValidationResult("You can't reserve more than the total number of seats", new[] { "ReservedSeats" });

            if (ReservedSeats > (Guests - TotalNumberOfAttendeeGuests)) // This potentially ignores tickets in progress so this is checked at Event Update
                yield return new ValidationResult("You can't reserve more than the total number of currently available seats", new[] { "ReservedSeats" });

            //if (MinTicketsAllowed < 1)
            //    yield return new ValidationResult("The number of minimum tickets must be 1 or more", new[] { "MinTicketsAllowed" });

            //if (MaxTicketsAllowed > Guests - ReservedSeats)
            //    yield return new ValidationResult("The number of maximum tickets can not be more than total seats available for sale", new[] { "MaxTicketsAllowed" });
        }

        #endregion

        #region Navigation Properties

        public virtual List<Review> Reviews { get; set; }

        public virtual SupperClub SupperClub
        {
            get { return _supperClub; }
            set
            {
                if (!ReferenceEquals(_supperClub, value))
                {
                    var previousValue = _supperClub;
                    _supperClub = value;
                    //FixupSupperClub(previousValue);
                }
            }
        }
        private SupperClub _supperClub;

        public virtual ICollection<EventAttendee> EventAttendees
        {
            get
            {
                if (_eventAttendees == null)
                {
                    var newCollection = new FixupCollection<EventAttendee>();
                    newCollection.CollectionChanged += FixupEventAttendees;
                    _eventAttendees = newCollection;
                }
                return _eventAttendees;
            }
            set
            {
                if (!ReferenceEquals(_eventAttendees, value))
                {
                    var previousValue = _eventAttendees as FixupCollection<EventAttendee>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventAttendees;
                    }
                    _eventAttendees = value;
                    var newValue = value as FixupCollection<EventAttendee>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventAttendees;
                    }
                }
            }
        }
        private ICollection<EventAttendee> _eventAttendees;

        public virtual ICollection<EventSeating> EventSeatings
        {
            get
            {
                if (_eventSeatings == null)
                {
                    var newCollection = new FixupCollection<EventSeating>();
                    newCollection.CollectionChanged += FixupEventSeatings;
                    _eventSeatings = newCollection;
                }
                return _eventSeatings;
            }
            set
            {
                if (!ReferenceEquals(_eventSeatings, value))
                {
                    var previousValue = _eventSeatings as FixupCollection<EventSeating>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventSeatings;
                    }
                    _eventSeatings = value;
                    var newValue = value as FixupCollection<EventSeating>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventSeatings;
                    }
                }
            }
        }
        private ICollection<EventSeating> _eventSeatings;

        public virtual ICollection<EventMenuOption> EventMenuOptions
        {
            get
            {
                if (_eventMenuOptions == null)
                {
                    var newCollection = new FixupCollection<EventMenuOption>();
                    newCollection.CollectionChanged += FixupEventMenuOptions;
                    _eventMenuOptions = newCollection;
                }
                return _eventMenuOptions;
            }
            set
            {
                if (!ReferenceEquals(_eventMenuOptions, value))
                {
                    var previousValue = _eventMenuOptions as FixupCollection<EventMenuOption>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventMenuOptions;
                    }
                    _eventMenuOptions = value;
                    var newValue = value as FixupCollection<EventMenuOption>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventMenuOptions;
                    }
                }
            }
        }
        private ICollection<EventMenuOption> _eventMenuOptions;

        public virtual ICollection<EventCuisine> EventCuisines
        {
            get
            {
                if (_eventCuisines == null)
                {
                    var newCollection = new FixupCollection<EventCuisine>();
                    newCollection.CollectionChanged += FixupEventCuisines;
                    _eventCuisines = newCollection;
                }
                return _eventCuisines;
            }
            set
            {
                if (!ReferenceEquals(_eventCuisines, value))
                {
                    var previousValue = _eventCuisines as FixupCollection<EventCuisine>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventCuisines;
                    }
                    _eventCuisines = value;
                    var newValue = value as FixupCollection<EventCuisine>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventCuisines;
                    }
                }
            }
        }
        private ICollection<EventCuisine> _eventCuisines;

        public virtual ICollection<EventArea> EventAreas
        {
            get
            {
                if (_eventAreas == null)
                {
                    var newCollection = new FixupCollection<EventArea>();
                    newCollection.CollectionChanged += FixupEventAreas;
                    _eventAreas = newCollection;
                }
                return _eventAreas;
            }
            set
            {
                if (!ReferenceEquals(_eventAreas, value))
                {
                    var previousValue = _eventAreas as FixupCollection<EventArea>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventAreas;
                    }
                    _eventAreas = value;
                    var newValue = value as FixupCollection<EventArea>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventAreas;
                    }
                }
            }
        }
        private ICollection<EventArea> _eventAreas;
        public virtual ICollection<EventDiet> EventDiets
        {
            get
            {
                if (_eventDiets == null)
                {
                    var newCollection = new FixupCollection<EventDiet>();
                    newCollection.CollectionChanged += FixupEventDiets;
                    _eventDiets = newCollection;
                }
                return _eventDiets;
            }
            set
            {
                if (!ReferenceEquals(_eventDiets, value))
                {
                    var previousValue = _eventDiets as FixupCollection<EventDiet>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventDiets;
                    }
                    _eventDiets = value;
                    var newValue = value as FixupCollection<EventDiet>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventDiets;
                    }
                }
            }
        }
        private ICollection<EventDiet> _eventDiets;
        public virtual ICollection<EventCity> EventCities
        {
            get
            {
                if (_eventCities == null)
                {
                    var newCollection = new FixupCollection<EventCity>();
                    newCollection.CollectionChanged += FixupEventCities;
                    _eventCities = newCollection;
                }
                return _eventCities;
            }
            set
            {
                if (!ReferenceEquals(_eventCities, value))
                {
                    var previousValue = _eventCities as FixupCollection<EventCity>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventCities;
                    }
                    _eventCities = value;
                    var newValue = value as FixupCollection<EventCity>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventCities;
                    }
                }
            }
        }
        private ICollection<EventCity> _eventCities;
        public virtual ICollection<EventImage> EventImages
        {
            get
            {
                if (_eventImages == null)
                {
                    var newCollection = new FixupCollection<EventImage>();
                    newCollection.CollectionChanged += FixupEventImages;
                    _eventImages = newCollection;
                }
                return _eventImages;
            }
            set
            {
                if (!ReferenceEquals(_eventImages, value))
                {
                    var previousValue = _eventImages as FixupCollection<EventImage>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventImages;
                    }
                    _eventImages = value;
                    var newValue = value as FixupCollection<EventImage>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventImages;
                    }
                }
            }
        }
        private ICollection<EventImage> _eventImages;

        public virtual ICollection<EventTag> EventTags
        {
            get
            {
                if (_eventTags == null)
                {
                    var newCollection = new FixupCollection<EventTag>();
                    newCollection.CollectionChanged += FixupEventTags;
                    _eventTags = newCollection;
                }
                return _eventTags;
            }
            set
            {
                if (!ReferenceEquals(_eventTags, value))
                {
                    var previousValue = _eventDiets as FixupCollection<EventTag>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventTags;
                    }
                    _eventTags = value;
                    var newValue = value as FixupCollection<EventTag>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventTags;
                    }
                }
            }
        }
        private ICollection<EventTag> _eventTags;

        public virtual ICollection<EventRecommendation> EventRecommendations
        {
            get
            {
                if (_eventRecommendations == null)
                {
                    var newCollection = new FixupCollection<EventRecommendation>();
                    newCollection.CollectionChanged += FixupEventRecommendations;
                    _eventRecommendations = newCollection;
                }
                return _eventRecommendations;
            }
            set
            {
                if (!ReferenceEquals(_eventRecommendations, value))
                {
                    var previousValue = _eventRecommendations as FixupCollection<EventRecommendation>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventRecommendations;
                    }
                    _eventRecommendations = value;
                    var newValue = value as FixupCollection<EventRecommendation>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventRecommendations;
                    }
                }
            }
        }
        private ICollection<EventRecommendation> _eventRecommendations;

        public virtual ICollection<Menu> Menus
        {
            get
            {
                if (_menus == null)
                {
                    var newCollection = new FixupCollection<Menu>();
                    newCollection.CollectionChanged += FixupMenus;
                    _menus = newCollection;
                }
                return _menus;
            }
            set
            {
                if (!ReferenceEquals(_menus, value))
                {
                    var previousValue = _menus as FixupCollection<Menu>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupMenus;
                    }
                    _menus = value;
                    var newValue = value as FixupCollection<Menu>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupMenus;
                    }
                }
            }
        }
        private ICollection<Menu> _menus;

        public virtual ICollection<EventWaitList> EventWaitLists
        {
            get
            {
                if (_eventWaitlists == null)
                {
                    var newCollection = new FixupCollection<EventWaitList>();
                    newCollection.CollectionChanged += FixupEventWaitlists;
                    _eventWaitlists = newCollection;
                }
                return _eventWaitlists;
            }
            set
            {
                if (!ReferenceEquals(_eventWaitlists, value))
                {
                    var previousValue = _eventWaitlists as FixupCollection<EventWaitList>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventWaitlists;
                    }
                    _eventWaitlists = value;
                    var newValue = value as FixupCollection<EventWaitList>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventWaitlists;
                    }
                }
            }
        }
        private ICollection<EventWaitList> _eventWaitlists;

        public virtual ICollection<EventVoucher> EventVouchers
        {
            get
            {
                if (_eventVouchers == null)
                {
                    var newCollection = new FixupCollection<EventVoucher>();
                    newCollection.CollectionChanged += FixupEventVouchers;
                    _eventVouchers = newCollection;
                }
                return _eventVouchers;
            }
            set
            {
                if (!ReferenceEquals(_eventVouchers, value))
                {
                    var previousValue = _eventVouchers as FixupCollection<EventVoucher>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventVouchers;
                    }
                    _eventVouchers = value;
                    var newValue = value as FixupCollection<EventVoucher>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventVouchers;
                    }
                }
            }
        }
        private ICollection<EventVoucher> _eventVouchers;

        #endregion

        #region Association Fixup

        //private void FixupSupperClub(SupperClub previousValue)
        //{
        //    if (previousValue != null && previousValue.Events.Contains(this))
        //    {
        //        previousValue.Events.Remove(this);
        //    }

        //    if (SupperClub != null)
        //    {
        //        if (!SupperClub.Events.Contains(this))
        //        {
        //            SupperClub.Events.Add(this);
        //        }
        //        if (SupperClubId != SupperClub.Id)
        //        {
        //            SupperClubId = SupperClub.Id;
        //        }
        //    }
        //}

        private void FixupEventAttendees(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventAttendee item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventAttendee item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventCuisines(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventCuisine item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventCuisine item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }
        private void FixupEventAreas(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventArea item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventArea item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }
        private void FixupEventSeatings(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventSeating item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventSeating item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventMenuOptions(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventMenuOption item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventMenuOption item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventDiets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventDiet item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventDiet item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventCities(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventCity item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventCity item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventImages(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventImage item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventImage item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventTags(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventTag item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventTag item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventRecommendations(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventRecommendation item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventRecommendation item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupMenus(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Menu item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (Menu item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventWaitlists(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventWaitList item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventWaitList item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }

        private void FixupEventVouchers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventVoucher item in e.NewItems)
                {
                    item.Event = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventVoucher item in e.OldItems)
                {
                    if (ReferenceEquals(item.Event, this))
                    {
                        item.Event = null;
                    }
                }
            }
        }
        #endregion
        # region Utils
        private static int findStringNthIndex(string target, string value, int n)
        {
            int defaultIndex = -1;
            Match m = Regex.Match(target, "((" + value + ").*?){" + n + "}");

            if (m.Success)
                defaultIndex = m.Groups[2].Captures[n - 1].Index;

            return defaultIndex;
        }
        #endregion
    }
    public class EventViewed
    {
        int EventId
        {
            get;
            set;
        }
        DateTime LastViewDate
        {
            get;
            set;
        }
    }    
}
