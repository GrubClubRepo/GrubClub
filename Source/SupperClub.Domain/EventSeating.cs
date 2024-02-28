//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace SupperClub.Domain 
{
    public class EventSeating
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }
    
        public virtual int EventId
        {
            get { return _eventId; }
            set
            {
                if (_eventId != value)
                {
                    if (Event != null && Event.Id != value)
                    {
                        Event = null;
                    }
                    _eventId = value;
                }
            }
        }
        private int _eventId;

        public virtual System.DateTime Start
        {
            get;
            set;
        }

        public virtual System.DateTime End
        {
            get;
            set;
        }
        public virtual int Guests
        {
            get;
            set;
        }
        public virtual int ReservedSeats
        {
            get;
            set;
        }
        public virtual bool IsDefault
        {
            get;
            set;
        }
        public virtual DateTime DateCreated
        {
            get;
            set;
        }
        //public virtual bool MinMaxBookingEnabled
        //{
        //    get;
        //    set;
        //}
        //public virtual bool SeatSelectionInMultipleOfMin
        //{
        //    get;
        //    set;
        //}
        //public virtual int MinTicketsAllowed
        //{
        //    get;
        //    set;
        //}
        //public virtual int MaxTicketsAllowed
        //{
        //    get;
        //    set;
        //}
        //public virtual bool MinGuestCountRequired
        //{
        //    get;
        //    set;
        //}
        //public virtual int MinGuestCount
        //{
        //    get;
        //    set;
        //}
        #region Calculated Values (Not in Db)
        /// <summary>
        /// Gets total number of seats booked by all Attendees.
        /// </summary>
        public int TotalNumberOfAttendeeGuests
        {
            get
            {
                int i = 0;
                foreach (EventAttendee e in this.EventAttendees)
                {
                    i += e.NumberOfGuests;
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
                return (this.Guests - this.ReservedSeats - this.TotalNumberOfAttendeeGuests);
            }
        }
        /// <summary>
        /// Gets availble seats.
        /// </summary>
        public int AvailableSeats
        {
            get
            {
                return (this.Guests - this.ReservedSeats - this.TotalNumberOfAttendeeGuests);
            }
            set
            {
                _availableSeats = value;
            }
        }
        private int _availableSeats;

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

            //if(MaxTicketsAllowed > Guests - ReservedSeats)
            //    yield return new ValidationResult("The number of maximum tickets can not be more than total seats available for sale", new[] { "MaxTicketsAllowed" });

        }

        #endregion
        #region Navigation Properties

        public virtual Event Event
        {
            get { return _event; }
            set
            {
                if (!ReferenceEquals(_event, value))
                {
                    var previousValue = _event;
                    _event = value;
                    FixupEvent(previousValue);
                }
            }
        }
        private Event _event;

        //public virtual ICollection<EventAttendee> EventAttendees
        //{
        //    get
        //    {
        //        if (_eventAttendees == null)
        //        {
        //            var newCollection = new FixupCollection<EventAttendee>();
        //            newCollection.CollectionChanged += FixupEventAttendees;
        //            _eventAttendees = newCollection;
        //        }
        //        return _eventAttendees;
        //    }
        //    set
        //    {
        //        if (!ReferenceEquals(_eventAttendees, value))
        //        {
        //            var previousValue = _eventAttendees as FixupCollection<EventAttendee>;
        //            if (previousValue != null)
        //            {
        //                previousValue.CollectionChanged -= FixupEventAttendees;
        //            }
        //            _eventAttendees = value;
        //            var newValue = value as FixupCollection<EventAttendee>;
        //            if (newValue != null)
        //            {
        //                newValue.CollectionChanged += FixupEventAttendees;
        //            }
        //        }
        //    }
        //}
        //private ICollection<EventAttendee> _eventAttendees;
        public virtual List<EventAttendee> EventAttendees { get; set; }
        #endregion
        #region Association Fixup
    
        private void FixupEvent(Event previousValue)
        {
            if (previousValue != null && previousValue.EventSeatings.Contains(this))
            {
                previousValue.EventSeatings.Remove(this);
            }
    
            if (Event != null)
            {
                if (!Event.EventSeatings.Contains(this))
                {
                    Event.EventSeatings.Add(this);
                }
                if (EventId != Event.Id)
                {
                    EventId = Event.Id;
                }
            }
        }
        //private void FixupEventAttendees(object sender, NotifyCollectionChangedEventArgs e)
        //{
        //    if (e.NewItems != null)
        //    {
        //        foreach (EventAttendee item in e.NewItems)
        //        {
        //            item.EventSeating = this;
        //        }
        //    }

        //    if (e.OldItems != null)
        //    {
        //        foreach (EventAttendee item in e.OldItems)
        //        {
        //            if (ReferenceEquals(item.EventSeating, this))
        //            {
        //                item.EventSeating = null;
        //            }
        //        }
        //    }
        //}
       
        #endregion
    }
}
