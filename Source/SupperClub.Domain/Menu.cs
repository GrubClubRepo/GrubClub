using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class Menu
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

        [Display(Name = "What's on the menu?")]
        public virtual string MenuItem
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
            if (previousValue != null && previousValue.Menus.Contains(this))
            {
                previousValue.Menus.Remove(this);
            }

            if (Event != null)
            {
                if (!Event.Menus.Contains(this))
                {
                    Event.Menus.Add(this);
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
