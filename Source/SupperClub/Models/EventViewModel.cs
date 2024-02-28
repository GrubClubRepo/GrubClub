using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;
using DataAnnotationsExtensions;
using CodeFirstStoredProcs;

namespace SupperClub.Models
{
    public class EventViewModel
    {
        public Event Event { get; set; }
        public RegisterModel RegisterModel { get; set; }
        public LogOnModel LogOnModel { get; set; }
        public string RedirectURL { get; set; }
        public int NumberOfTickets { get; set; }
    }

    public class MasterEventModel
    {
        public Event MasterEvent { get; set; }
        public List<ChildEvent> FutureEventList { get; set; }
        public string LowestPrice { get; set; }
    }
    
    public class ChildEvent
    {
        public int EventId { get; set; }
        public string EventUrl {get; set;}
        public DateTime EventDate { get; set; }
        public bool Soldout { get; set; }
    }

    public class EventCloneModel
    {
        public int EventId { get; set; }

        [Required(ErrorMessage = "Date of Event Required")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime EventDate { get; set; }

        [Required(ErrorMessage = "Start Time Required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "Duration Required")]
        [Min(1, ErrorMessage = "Please provide a valid duration")]
        public double Duration
        {
            get;
            set;
        }
        [Required]
        public bool MultiSeating { get; set; }
    }
    
}