using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CodeFirstStoredProcs;
using System.Web.Mvc;
using System.Web;

namespace SupperClub.Domain
{
    public enum SupperClubStatus
    {
        Pro = 1,
        Amateur = 2,
        DontKnow = 3
    }

    public class SupperClub
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }
        public virtual DateTime? DateCreated
        {
            get;
            set;
        }
        public virtual System.Guid UserId
        {
            get { return _userId; }
            set
            {
                if (_userId != value)
                {
                    if (User != null && User.Id != value)
                    {
                        User = null;
                    }
                    _userId = value;
                }
            }
        }
        private System.Guid _userId;

        [Required(ErrorMessage = "Club Name Required")]
        [StringLength(500, ErrorMessage = "500 chars max")]
        [Display(Name = "Club Name *")]
        [Remote("HostNameAvailable", "Host", ErrorMessage = "Name is already in use!")]
        public virtual string Name
        {
            get;
            set;
        }

        [Display(Name = "Club Name Url")]
        public virtual string UrlFriendlyName
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Summary Required")]
        [Display(Name = "Summary *")]
        [StringLength(500, ErrorMessage = "Your Summary can only be 500 Characters. Get Summarising!")]
        [DataType(DataType.MultilineText)]
        public virtual string Summary
        {
            get;
            set;
        }

        [DataType(DataType.MultilineText)]
        public virtual string Description
        {
            get;
            set;
        }

        // [Url(ErrorMessage = "http://...")]
        [Display(Name = "Blog", Prompt = "www.myblog.com")]
        public virtual string Blog
        {
            get;
            set;
        }
        // [Url(ErrorMessage = "http://...")]
        [Display(Name="Twitter Name", Prompt="@MyTwitterName")]
        public virtual string Twitter
        {
            get;
            set;
        }
        // [Url(ErrorMessage = "http://...")]
        [Display(Name = "Facebook Page", Prompt = "www.facebook.com/MyPage")]
        public virtual string Facebook
        {
            get;
            set;
        }
        // [Url(ErrorMessage = "http://...")]
        [Display(Name = "Pinterest Page", Prompt = "www.pinterest.com/MyPage")]
        public virtual string Pinterest
        {
            get;
            set;
        }

        [Display(Name = "Instagram", Prompt = "www.instagram.com/MyPage")]
        public virtual string Instagram
        {
            get;
            set;
        }

        [Display(Name = "Trading Name")]
        public virtual string TradingName
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Address 1 Required")]
        [Display(Name = "Address 1 *")]
        public virtual string Address
        {
            get;
            set;
        }

        [Display(Name = "Address 2")]
        public virtual string Address2
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Town/City Required")]
        [Display(Name = "Town/City *")]
        public virtual string City
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Country Required")]
        [Display(Name = "Country *")]
        public virtual string Country
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Post Code Required")]
        [Display(Name = "Post Code *")]
        public virtual string PostCode
        {
            get;
            set;
        }

        public virtual double Latitude
        {
            get;
            set;
        }


        public virtual double Longitude
        {
            get;
            set;
        }

        [Required(ErrorMessage = "Primary Contact Number Required")]
        [Display(Name = "Primary Contact Number *")]
        public virtual string ContactNumber
        {
            get;
            set;
        }

        [Display(Name = "VAT Number (This is not a required field)")]
        public virtual string VATNumber
        {
            get;
            set;
        }

        [Display(Name = "Name on Cheque")]
        public virtual string ChequeName
        {
            get;
            set;
        }

        [Display(Name = "Bank Name")]
        public virtual string BankName
        {
            get;
            set;
        }

        [Display(Name = "Bank Address Name")]
        public virtual string BankAddress
        {
            get;
            set;
        }

        [Display(Name = "Sort Code")]
        public virtual string SortCode
        {
            get;
            set;
        }


        [Display(Name = "Account Number")]
        public virtual string AccountNumber
        {
            get;
            set;
        }

        [Display(Name = "House Rules")]
        [DataType(DataType.MultilineText)]
        public virtual string HouseRule
        {
            get;
            set;
        }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Have you Registered the Premises with your Council?")]
        public virtual bool CouncilRegistered
        {
            get;
            set;
        }
        [Required(ErrorMessage = "*")]
        [Display(Name = "Do you have a Food Hygiene Certificate?")]
        public virtual bool FoodHygieneCertificate
        {
            get;
            set;
        }
        //[Display(Name = "Food Hygiene Certificate Number")]
        //public virtual string FoodHygieneCertificateNumber
        //{
        //    get;
        //    set;
        //}
        [Required(ErrorMessage = "*")]
        [Display(Name = "Do you have Public Liability Insurance?")]
        public virtual bool IndemnityInsurace
        {
            get;
            set;
        }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Do you have an Alcohol License?")]
        public virtual bool AlcoholLicense
        {
            get;
            set;
        }

        public virtual string ImagePath
        {
            get;
            set;
        }

        public virtual string ImagePaths
        {
            get
            {
                if (imagePaths == null || imagePaths == "")
                {
                    imagePaths = "";
                    foreach (SupperClubImage si in this.SupperClubImages)
                        imagePaths += si.ImagePath + ",";
                    if (imagePaths.Length > 0)
                        imagePaths = imagePaths.Substring(0, imagePaths.Length - 1);
                }
                return imagePaths;
            }
            set
            {
                imagePaths = value;
            }
        }
        private string imagePaths;
        public virtual bool Active
        {
            get;
            set;
        }

        public virtual int Status
        {
            get;
            set;
        }
        //public virtual bool MultipleNotificationEmailIds
        //{
        //    get;
        //    set;
        //}
        public List<Event> FutureEvents
        {
            get
            {
                return this.Events.FindAll(x => x.Start > DateTime.Now);
            }
        }

        public List<Event> PastEvents
        {
            get
            {
                return this.Events.FindAll(x => x.Start < DateTime.Now);
            }
        }

        /// <summary>
        /// Gets average rank from all Events that have a ranking.
        /// Returns null if no ranking data available.
        /// </summary>
        public decimal? AverageRank
        {
            get
            {
                decimal averageRank = 0;
                decimal totalScore = 0;
                int numberGuestsRanked = 0;
                if (this.Events != null)
                {
                    foreach (Event e in this.Events)
                    {
                        //if (!e.Private && e.AverageRank != null)
                        //if (e.AverageRank != null)
                        //{
                        //    totalScore += (int)e.AverageRank;
                        //    numberEventsRanked++;
                        //}
                        if(e.Reviews != null && e.Reviews.Count > 0)
                        {
                            foreach(Review r in e.Reviews)
                            {
                                if (r.Rating != null)
                                {
                                    totalScore += (decimal)r.Rating;
                                    numberGuestsRanked++;
                                }
                            }
                        }
                    }
                }

                if (numberGuestsRanked > 0)
                {
                    averageRank = Math.Round((totalScore / (decimal)numberGuestsRanked), MidpointRounding.AwayFromZero);
                }

                return averageRank;
            }
        }

        /// <summary>
        /// Gets total number of all Attendees that provided a ranking on all events.
        /// </summary>
        public int NumberGuestRanks
        {
            get
            {
                int numberGuestRanks = 0;
                if (this.Events != null)
                {
                    foreach (Event e in this.Events)
                    {
                        //if (!e.Private && e.Reviews != null && e.Reviews.Count > 0)
                        if (e.Reviews != null && e.Reviews.Count > 0)
                        {
                            foreach (Review r in e.Reviews)
                            {
                                if(r.Rating != null)
                                    numberGuestRanks++;
                            }
                        }
                    }
                }
                return numberGuestRanks;
            }
        }

        
        public int NumberOfReviews
        {
            get
            {
                int numberGuestReviews = 0;
                if (this.Events != null && this.Events.Count > 0)
                {
                    foreach (Event e in this.Events)
                    {
                        //if (!e.Private && e.Reviews != null && e.Reviews.Count > 0)
                        if (e.Reviews != null && e.Reviews.Count > 0)
                        {
                            foreach (Review r in e.Reviews)
                            {
                                //if (r.PublicReview != null && r.PublicReview.Length > 0)
                                    numberGuestReviews++;
                            }                              
                        }
                    }
                }
                return numberGuestReviews;
            }
        }
        [Display(Name = "Local file")]
        public HttpPostedFileBase File { get; set; }

        public virtual List<int> SelectedProfileTagIds
        {
            get
            {
                if (profileTagIds != null && profileTagIds.Count == 0 && this.SupperClubProfileTags != null)
                {
                    foreach (SupperClubProfileTag t in this.SupperClubProfileTags)
                        profileTagIds.Add(t.ProfileTag.Id);
                }
                return profileTagIds;
            }
            set
            {
                profileTagIds = value;
            }
        }
        private List<int> profileTagIds = new List<int>();

        #endregion

        // These are cleaned up navigation properties

        #region Navigation Properties

        public virtual List<Event> Events { get; set; }

        public virtual User User { get; set; }

        public virtual ICollection<SupperClubImage> SupperClubImages
        {
            get
            {
                if (_supperClubImages == null)
                {
                    var newCollection = new FixupCollection<SupperClubImage>();
                    newCollection.CollectionChanged += FixupSupperClubImages;
                    _supperClubImages = newCollection;
                }
                return _supperClubImages;
            }
            set
            {
                if (!ReferenceEquals(_supperClubImages, value))
                {
                    var previousValue = _supperClubImages as FixupCollection<SupperClubImage>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSupperClubImages;
                    }
                    _supperClubImages = value;
                    var newValue = value as FixupCollection<SupperClubImage>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSupperClubImages;
                    }
                }
            }
        }
        private ICollection<SupperClubImage> _supperClubImages;

        public virtual ICollection<SupperClubVoucher> SupperClubVouchers
        {
            get
            {
                if (_supperClubVouchers == null)
                {
                    var newCollection = new FixupCollection<SupperClubVoucher>();
                    newCollection.CollectionChanged += FixupSupperClubVouchers;
                    _supperClubVouchers = newCollection;
                }
                return _supperClubVouchers;
            }
            set
            {
                if (!ReferenceEquals(_supperClubVouchers, value))
                {
                    var previousValue = _supperClubVouchers as FixupCollection<SupperClubVoucher>;
                    if (previousValue != null)
                    {
                        previousValue.CollectionChanged -= FixupSupperClubVouchers;
                    }
                    _supperClubVouchers = value;
                    var newValue = value as FixupCollection<SupperClubVoucher>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSupperClubVouchers;
                    }
                }
            }
        }
        private ICollection<SupperClubVoucher> _supperClubVouchers;

        public virtual ICollection<SupperClubProfileTag> SupperClubProfileTags
        {
            get
            {
                if (_supperClubProfileTags == null)
                {
                    var newCollection = new FixupCollection<SupperClubProfileTag>();
                    newCollection.CollectionChanged += FixupSupperClubProfileTags;
                    _supperClubProfileTags = newCollection;
                }
                return _supperClubProfileTags;
            }
            set
            {
                if (!ReferenceEquals(_supperClubProfileTags, value))
                {

                    var newValue = value as FixupCollection<SupperClubProfileTag>;
                    if (newValue != null)
                    {
                        newValue.CollectionChanged += FixupSupperClubProfileTags;
                    }
                }
            }
        }
        private ICollection<SupperClubProfileTag> _supperClubProfileTags;

        public virtual string SupperClubProfileTagList
        {
            get
            {
                if (supperClubProfileTagList == null || supperClubProfileTagList == "")
                {
                    supperClubProfileTagList = "";
                    if (this.profileTagIds != null)
                    {
                        foreach (int et in this.profileTagIds)
                            supperClubProfileTagList += et.ToString() + ",";
                        if (supperClubProfileTagList.Length > 0)
                            supperClubProfileTagList = supperClubProfileTagList.Substring(0, supperClubProfileTagList.Length - 1);
                    }
                }
                return supperClubProfileTagList;
            }
            set
            {
                supperClubProfileTagList = value;
            }
        }
        private string supperClubProfileTagList;
        #endregion

        #region Association Fixup
        // Fix up properties can be removed as this is handled by the database
 

        private void FixupSupperClubProfileTags(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SupperClubProfileTag item in e.NewItems)
                {
                    item.SupperClub = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SupperClubProfileTag item in e.OldItems)
                {
                    if (ReferenceEquals(item.SupperClub, this))
                    {
                        item.SupperClub = null;
                    }
                }
            }
        }

        private void FixupSupperClubImages(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SupperClubImage item in e.NewItems)
                {
                    item.SupperClub = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SupperClubImage item in e.OldItems)
                {
                    if (ReferenceEquals(item.SupperClub, this))
                    {
                        item.SupperClub = null;
                    }
                }
            }
        }
        private void FixupSupperClubVouchers(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (SupperClubVoucher item in e.NewItems)
                {
                    item.SupperClub = this;
                }
            }

            if (e.OldItems != null)
            {
                foreach (SupperClubVoucher item in e.OldItems)
                {
                    if (ReferenceEquals(item.SupperClub, this))
                    {
                        item.SupperClub = null;
                    }
                }
            }
        }
        #endregion
    }

    public class SupperClubs
    {
        [StoredProcAttributes.Name("GrubClubName")]
        public string GrubClubName { get; set; }
        [StoredProcAttributes.Name("GrubClubUrlFriendlyName")]
        public string GrubClubUrlFriendlyName { get; set; }

        [StoredProcAttributes.Name("LastName")]
        public string LastName { get; set; }    

        [StoredProcAttributes.Name("FirstName")]
        public string FirstName { get; set; }

        [StoredProcAttributes.Name("SupperclubId")]
        public int SupperclubId { get; set; }

        [StoredProcAttributes.Name("BrandNew")]
        public string BrandNew { get; set; }

        [StoredProcAttributes.Name("HasFutureEvents")]
        public bool HasFutureEvents { get; set; }
        
        [StoredProcAttributes.Name("ReviewCount")]
        public int ReviewCount { get; set; }

        [StoredProcAttributes.Name("Rating")]
        private decimal rating;
        public Decimal Rating
        {
            get
            {
                return Math.Round(rating, MidpointRounding.AwayFromZero);
            }
            set
            {
                rating = value;
            }
        }
        [StoredProcAttributes.Name("FutureEventCount")]
        public int FutureEventCount { get; set; }
        
        [StoredProcAttributes.Name("ImagePath")]
        public string ImagePath { get; set; }
        
        [StoredProcAttributes.Name("Description")]
        public string Description { get; set; }

        [StoredProcAttributes.Name("Summary")]
        public string Summary { get; set; }

        [StoredProcAttributes.Name("Active")]
        public bool Active { get; set; }

        [StoredProcAttributes.Name("FollowChef")]
        public int FollowChef { get; set; }

        [StoredProcAttributes.Name("Followers")]
        public int Followers { get; set; }


    }

    public class SearchSupperClub
    {
        [StoredProcAttributes.Name("Name")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.NVarChar)]
        public string Name { get; set; }

        [StoredProcAttributes.Name("UserId")]
        [StoredProcAttributes.ParameterType(System.Data.SqlDbType.UniqueIdentifier)]
        public Guid? UserId { get; set; }
    }
}
