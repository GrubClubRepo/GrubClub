using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public enum ProfileTagTargetUser
    {
        Admin = 1,
        Host = 2        
    }

    public class ProfileTag
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

        [Display(Name = "Is it a Private Tag")]
        public virtual bool Private
        {
            get;
            set;
        }

        public virtual int TargetUser
        {
            get;
            set;
        }

        public virtual DateTime CreatedDate
        {
            get;
            set;
        }

        #endregion
        #region Navigation Properties
        

        public virtual ICollection<SupperClubProfileTag> SupperClubTags
        {
            get
            {
                if (_supperClubProfileTag == null)
                {
                    var newCollection = new FixupCollection<SupperClubProfileTag>();
                    newCollection.CollectionChanged += FixupEventTags;
                    _supperClubProfileTag = newCollection;
                }
                return _supperClubProfileTag;
            }
            set
            {
                if (!ReferenceEquals(_supperClubProfileTag, value))
                {
                    var previousValue = _supperClubProfileTag as FixupCollection<EventTag>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventTags;
                    }
                    _supperClubProfileTag = value;
                    var newValue = value as FixupCollection<EventTag>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventTags;
                    }
                }
            }
        }
        private ICollection<SupperClubProfileTag> _supperClubProfileTag;

        #endregion
        #region Association Fixup

        private void FixupEventTags(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SupperClubProfileTag item in e.NewItems)
                {
                    item.ProfileTag = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SupperClubProfileTag item in e.OldItems)
                {
                    if (ReferenceEquals(item.ProfileTag, this))
                    {
                        item.ProfileTag = null;
                    }
                }
            }
        }

        #endregion
    }
}
