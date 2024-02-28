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
    public class EventImage
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

        public virtual string ImagePath
        {
            get;
            set;
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

        #endregion
        #region Association Fixup

        private void FixupEvent(Event previousValue)
        {
            if (previousValue != null && previousValue.EventImages.Contains(this))
            {
                previousValue.EventImages.Remove(this);
            }

            if (Event != null)
            {
                if (!Event.EventImages.Contains(this))
                {
                    Event.EventImages.Add(this);
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