﻿//------------------------------------------------------------------------------
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
    public class EventCity
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

        public virtual int CityId
        {
            get { return _cityId; }
            set
            {
                if (_cityId != value)
                {
                    if (City != null && City.Id != value)
                    {
                        City = null;
                    }
                    _cityId = value;
                }
            }
        }
        private int _cityId;

        #endregion
        #region Navigation Properties

        public virtual City City
        {
            get { return _city; }
            set
            {
                if (!ReferenceEquals(_city, value))
                {
                    var previousValue = _city;
                    _city = value;
                    FixupCity(previousValue);
                }
            }
        }
        private City _city;

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

        private void FixupCity(City previousValue)
        {
            if (previousValue != null && previousValue.EventCities.Contains(this))
            {
                previousValue.EventCities.Remove(this);
            }

            if (City != null)
            {
                if (!City.EventCities.Contains(this))
                {
                    City.EventCities.Add(this);
                }
                if (CityId != City.Id)
                {
                    CityId = City.Id;
                }
            }
        }

        private void FixupEvent(Event previousValue)
        {
            if (previousValue != null && previousValue.EventCities.Contains(this))
            {
                previousValue.EventCities.Remove(this);
            }

            if (Event != null)
            {
                if (!Event.EventCities.Contains(this))
                {
                    Event.EventCities.Add(this);
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
