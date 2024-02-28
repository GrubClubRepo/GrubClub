using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;

namespace SupperClub.Models
{
    public class SearchModel
    {
        public string Location { get; set; }
        public string Keyword { get; set; }
        public string QueryTag { get; set; }
        public string QueryCity { get; set; }
        public string QueryArea { get; set; }
        public Guid? UserId { get; set; }

        [Required]
        [Min(1, ErrorMessage = "Please provide a valid number of guests")]
        public int? Guests { get; set; }

        [Display(Name = "Starting Date")]      
        public DateTime? StartDate { get; set; }
        
        public int Distance { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        
        [Display(Name = "Food Keyword")]
        public string FoodKeyword { get; set; }

        public bool? Charity { get; set; }
        [Display(Name = "Alcohol")]
        public bool? Alcohol { get; set; }

        public string NewChef { get; set; }
        [Display(Name = "Allergy Friendly")]
        public bool AllergyFriendly { get; set; }
        public IList<PriceRange> PriceRange { get; set; }
        public IList<Diet> Diet { get; set; }
        public IList<Cuisine> Cuisine { get; set; }

        public int[] PriceRangeOptions { get; set; }

        public int[] DietOptions { get; set; }

        public int[] CuisineOptions { get; set; }
        
        public string SelectedDiets { get; set; }

        public string SelectedCuisines { get; set; }

        public string SelectedTags { get; set; } 

        public int PageNumber { get; set; }
        private int _endDateOffset = 100;
        public int EndDateOffset
        {
            get
            {
                return _endDateOffset;
            }

            set
            {
                _endDateOffset = value;
            }
        }
        private int _sourcePageTypeId = (int)SearchSourcePageType.Search;
        public int SourcePageTypeId
        {
            get
            {
                return _sourcePageTypeId;
            }

            set
            {
                _sourcePageTypeId = value;
            }
        }

        public IList<Tag> Tag { get; set; }

        public int[] TagOptions { get; set; }

    }

    public class SearchCategoryModel
    {
        public string CategoryUrlFriendlyName;
        public IList<Tag> CategoryTags;
    }
 
}