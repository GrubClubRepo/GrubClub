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
    public class EventTag
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

        public virtual int TagId
        {
            get { return _tagId; }
            set
            {
                if (_tagId != value)
                {
                    if (Tag != null && Tag.Id != value)
                    {
                        Tag = null;
                    }
                    _tagId = value;
                }
            }
        }
        private int _tagId;

        #endregion
        #region Navigation Properties

        public virtual Tag Tag
        {
            get { return _tag; }
            set
            {
                if (!ReferenceEquals(_tag, value))
                {
                    var previousValue = _tag;
                    _tag = value;
                    FixupTag(previousValue);
                }
            }
        }
        private Tag _tag;

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

        private void FixupTag(Tag previousValue)
        {
            if (previousValue != null && previousValue.EventTags.Contains(this))
            {
                previousValue.EventTags.Remove(this);
            }

            if (Tag != null)
            {
                if (!Tag.EventTags.Contains(this))
                {
                    Tag.EventTags.Add(this);
                }
                if (TagId != Tag.Id)
                {
                    TagId = Tag.Id;
                }
            }
        }

        private void FixupEvent(Event previousValue)
        {
            if (previousValue != null && previousValue.EventTags.Contains(this))
            {
                previousValue.EventTags.Remove(this);
            }

            if (Event != null)
            {
                if (!Event.EventTags.Contains(this))
                {
                    Event.EventTags.Add(this);
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
