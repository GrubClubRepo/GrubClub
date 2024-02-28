using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public enum TargetUser
    {
        Admin = 1,
        Host = 2,
        AdminHost = 3

    }
    public class Tag
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Tag Name Required")]
        [Display(Name = "Tag")]
        public virtual string Name
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Tag SEO Friendly Name Required")]
        [Display(Name = "Tag SEO Name")]
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public virtual decimal? StartPrice
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Meta Title Required")]
        [Display(Name = "Meta Title")]
        [DataType(DataType.MultilineText)]
        public virtual string MetaTitle
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Meta Description Required")]
        [Display(Name = "Meta Description")]
        [DataType(DataType.MultilineText)]
        public virtual string MetaDescription
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Search Page Title Required")]
        [Display(Name = "H2 Tag Title")]
        [DataType(DataType.MultilineText)]
        public virtual string H2Tag
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Tag Description Required")]
        [Display(Name = "Tag Description")]
        [DataType(DataType.MultilineText)]
        public virtual string TagDescription
        {
            get;
            set;
        }
        #endregion
        #region Calculated Values (Not in Db)
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? CostToGuest
        {
            get
            {
                if (this.StartPrice != null)
                    return CostCalculator.CostToGuest((decimal)this.StartPrice, (decimal)(0.10));
                else
                    return null;
            }
        }

        [Display(Name = "Is it Private Tag")]

        public virtual bool Private
        {
            get;
            set;
        }


        [Display(Name = "Target User")]

        public virtual int TargetUser
        {
            get;
            set;
        }

        #endregion
        #region Navigation Properties
        //public virtual TileTag TileTag { get; set; }

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
                    var previousValue = _eventTags as FixupCollection<EventTag>;
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

        #endregion
        #region Association Fixup

        private void FixupEventTags(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventTag item in e.NewItems)
                {
                    item.Tag = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventTag item in e.OldItems)
                {
                    if (ReferenceEquals(item.Tag, this))
                    {
                        item.Tag = null;
                    }
                }
            }
        }

        #endregion
    }
}
