using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
   
    public class Area
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Display(Name = "Area")]
        public virtual string Name
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Area SEO Friendly Name Required")]
        [Display(Name = "Area SEO Name")]
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }
        public virtual int CityId
        {
            get;
            set;
        }
        [ForeignKey("CityId")]
        public virtual City City { get; set; }
        #endregion
        #region Navigation Properties

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

        #endregion
        #region Association Fixup

        private void FixupEventAreas(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventArea item in e.NewItems)
                {
                    item.Area = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventArea item in e.OldItems)
                {
                    if (ReferenceEquals(item.Area, this))
                    {
                        item.Area = null;
                    }
                }
            }
        }

        #endregion
    }
}
