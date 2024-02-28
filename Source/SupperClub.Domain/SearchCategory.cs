using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
   
    public class SearchCategory
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        [Display(Name = "Category")]
        public virtual string Name
        {
            get;
            set;
        }
        [Display(Name = "SEO Friendly Name")]
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }
        [Required(ErrorMessage = "Meta Title Required")]
        [Display(Name = "Meta Title")]
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
        [Required(ErrorMessage = "Category Description Required")]
        [Display(Name = "Text Description")]
        [DataType(DataType.MultilineText)]
        public virtual string Description
        {
            get;
            set;
        }
        public virtual List<int> SelectedTagIds
        {
            get
            {
                if (tagIds != null && tagIds.Count == 0 && this.SearchCategoryTags != null)
                {
                    foreach (SearchCategoryTag t in this.SearchCategoryTags)
                        tagIds.Add(t.TagId);
                }
                return tagIds;
            }
            set
            {
                tagIds = value;
            }
        }
        private List<int> tagIds = new List<int>();

        public virtual string TagList
        {
            get
            {
                if (tagList == null || tagList == "")
                {
                    tagList = "";
                    if (this.tagIds != null)
                    {
                        foreach (int et in this.tagIds)
                            tagList += et.ToString() + ",";
                        if (tagList.Length > 0)
                            tagList = tagList.Substring(0, tagList.Length - 1);
                    }
                }
                return tagList;
            }
            set
            {
                tagList = value;
            }
        }
        private string tagList;
        #endregion
        #region Navigation Properties
        public virtual ICollection<SearchCategoryTag> SearchCategoryTags
        {
            get
            {
                if (_searchCategoryTags == null)
                {
                    var newCollection = new FixupCollection<SearchCategoryTag>();
                    newCollection.CollectionChanged += FixupSearchCategoryTags;
                    _searchCategoryTags = newCollection;
                }
                return _searchCategoryTags;
            }
            set
            {
                if (!ReferenceEquals(_searchCategoryTags, value))
                {
                    var previousValue = _searchCategoryTags as FixupCollection<SearchCategoryTag>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSearchCategoryTags;
                    }
                    _searchCategoryTags = value;
                    var newValue = value as FixupCollection<SupperClubImage>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSearchCategoryTags;
                    }
                }
            }
        }
        private ICollection<SearchCategoryTag> _searchCategoryTags;

        #endregion
        #region Association Fixup

        // Fix up properties can be removed as this is handled by the database
        private void FixupSearchCategoryTags(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SearchCategoryTag item in e.NewItems)
                {
                    item.SearchCategory = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SearchCategoryTag item in e.OldItems)
                {
                    if (ReferenceEquals(item.SearchCategory, this))
                    {
                        item.SearchCategory = null;
                    }
                }
            }
        }

        #endregion
    }
}
