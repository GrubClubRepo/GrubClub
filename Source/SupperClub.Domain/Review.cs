using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class Review
    {
        public Review()
        {
            Publish = false;
            Anonymous = false;
        }

        public Review(string title, string publicReview, string privateReview) : this()
        {
            Title = title;
            PublicReview = publicReview;
            PrivateReview = privateReview;
        }

        public int Id { get; set; }

        [Display(Name = "Review Title")]
        [StringLength(80, ErrorMessage = "80 chars max")]
        public string Title { get; set; }

        [Display(Name = "Public Comments")]
        [DataType(DataType.MultilineText)]
        [Required]
        public string PublicReview { get; set; }

        [Display(Name = "Private Comments")]
        [DataType(DataType.MultilineText)]
        public string PrivateReview { get; set; }

        [Display(Name = "Publish on Event Page")]
        public bool Publish { get; set; }

        [Display(Name = "Post Anonymously?")]
        public bool Anonymous { get; set; }

        [Display(Name = "Food")]
        [Range(0, 5, ErrorMessage = "Must be between 0 and 5")]
        public decimal? FoodRating { get; set; }

        [Display(Name = "Ambience")]
        [Range(0, 5, ErrorMessage = "Must be between 0 and 5")]
        public decimal? AmbienceRating { get; set; }

        [Display(Name = "Rating")]
        [Required]
        [Range(0, 5, ErrorMessage = "Must be between 0 and 5")]
        public decimal? Rating { get; set; }

        public DateTime DateCreated { get; set; }

        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Display(Name = "Host's Comments")]
        [DataType(DataType.MultilineText)]
        public string HostResponse { get; set; }

        [Display(Name = "Admin's Comments")]
        [DataType(DataType.MultilineText)]
        public string AdminResponse { get; set; }
        public string Email { get; set; }
        public string GuestName { get; set; }
        public DateTime? HostResponseDate { get; set; }
        public DateTime? AdminResponseDate { get; set; }
    }
}
