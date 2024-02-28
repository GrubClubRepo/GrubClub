using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public enum DietType
    {
        Diet = 1,
        Allergy = 2,
        Others = 3
    }
    public class Diet
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Display(Name = "Diet")]
        public virtual string Name
        {
            get;
            set;
        }
        public virtual int DietTypeId
        {
            get;
            set;
        }
        #endregion
        #region Navigation Properties

        public virtual ICollection<EventDiet> EventDiets
        {
            get
            {
                if (_eventDiets == null)
                {
                    var newCollection = new FixupCollection<EventDiet>();
                    newCollection.CollectionChanged += FixupEventDiets;
                    _eventDiets = newCollection;
                }
                return _eventDiets;
            }
            set
            {
                if (!ReferenceEquals(_eventDiets, value))
                {
                    var previousValue = _eventDiets as FixupCollection<EventDiet>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupEventDiets;
                    }
                    _eventDiets = value;
                    var newValue = value as FixupCollection<EventDiet>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupEventDiets;
                    }
                }
            }
        }
        private ICollection<EventDiet> _eventDiets;

        #endregion
        #region Association Fixup

        private void FixupEventDiets(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (EventDiet item in e.NewItems)
                {
                    item.Diet = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (EventDiet item in e.OldItems)
                {
                    if (ReferenceEquals(item.Diet, this))
                    {
                        item.Diet = null;
                    }
                }
            }
        }

        #endregion
    }
}
