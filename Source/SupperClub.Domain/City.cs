using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
   
    public class City
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Display(Name = "City")]
        public virtual string Name
        {
            get;
            set;
        }
        [Required(ErrorMessage = "City SEO Friendly Name Required")]
        [Display(Name = "City SEO Name")]
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }
        #endregion
        #region Navigation Properties

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

        #endregion
        #region Association Fixup

        private void FixupEventCities(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventCity item in e.NewItems)
                {
                    item.City = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventCity item in e.OldItems)
                {
                    if (ReferenceEquals(item.City, this))
                    {
                        item.City = null;
                    }
                }
            }
        }

        #endregion
    }
}
