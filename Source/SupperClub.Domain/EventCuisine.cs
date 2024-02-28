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

namespace SupperClub.Domain
{
    public class EventCuisine
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
    
        public virtual int CuisineId
        {
            get { return _cuisineId; }
            set
            {
                if (_cuisineId != value)
                {
                    if (Cuisine != null && Cuisine.Id != value)
                    {
                        Cuisine = null;
                    }
                    _cuisineId = value;
                }
            }
        }
        private int _cuisineId;

        #endregion
        #region Navigation Properties
    
        public virtual Cuisine Cuisine
        {
            get { return _cuisine; }
            set
            {
                if (!ReferenceEquals(_cuisine, value))
                {
                    var previousValue = _cuisine;
                    _cuisine = value;
                    FixupCuisine(previousValue);
                }
            }
        }
        private Cuisine _cuisine;
    
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

        #endregion
        #region Association Fixup
    
        private void FixupCuisine(Cuisine previousValue)
        {
            if (previousValue != null && previousValue.EventCuisines.Contains(this))
            {
                previousValue.EventCuisines.Remove(this);
            }
    
            if (Cuisine != null)
            {
                if (!Cuisine.EventCuisines.Contains(this))
                {
                    Cuisine.EventCuisines.Add(this);
                }
                if (CuisineId != Cuisine.Id)
                {
                    CuisineId = Cuisine.Id;
                }
            }
        }
    
        private void FixupEvent(Event previousValue)
        {
            if (previousValue != null && previousValue.EventCuisines.Contains(this))
            {
                previousValue.EventCuisines.Remove(this);
            }
    
            if (Event != null)
            {
                if (!Event.EventCuisines.Contains(this))
                {
                    Event.EventCuisines.Add(this);
                }
                if (EventId != Event.Id)
                {
                    EventId = Event.Id;
                }
            }
        }

        #endregion
    }
}
