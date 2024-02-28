using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using SupperClub.Domain;

namespace SupperClub.Models
{
    public class ContactUsModel
    {
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string Message { get; set; }
    }
    public class NewsletterSubscriberModel
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email address")]
        [Required(ErrorMessage = "The email address is required!")]
        [DataType(DataType.EmailAddress, ErrorMessage = "Please enter a valid Email Address!")]
        public string Email { get; set; }

        public string Status { get; set; }
    }

    public class HomePage
    {
        public IList<Event> RecentlyAddedEvents { get; set; }

        public SearchModel HomeSearch { get; set; }  


    }
    public class NewHome
    {
        public IList<Event> RecentlyAddedEvents { get; set; }

        // public SearchModel HomeSearch { get; set; }

        public string Location { get; set; }

        public string DateString { get; set; }

        public IList<PressRelease> PressReleases { get; set; }

        public Event FavouriteEvent { get; set; }
    }
    public class NewHomePage
    {
        public IList<TileTag> TileCollection { get; set; }

        public SearchModel HomeSearch { get; set; }

        public IList<PressRelease> PressReleases { get; set; }

    }
 
   public  class NewCMSHomeModel
   {
      public IList<PageCMS> PageCMS { get; set;}
   }
    public class ChefsModel
    {
        public IList<SupperClubs> SupperClubs { get; set; }

        public RegisterModel RegisterModel { get; set; }
        public LogOnModel LogOnModel { get; set; }
        public string RedirectURL { get; set; }

    }
    public class PressReleaseModel
    {
        public IList<PressRelease> PressReleases { get; set; }
    }

   }
