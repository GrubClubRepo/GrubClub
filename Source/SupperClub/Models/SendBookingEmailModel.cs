using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.Web.Mvc;

namespace SupperClub.Models
{
    public class SendBookingEmailModel
    {
        public SendBookingEmailModel()
        {
            EmailAddress = "";
            Comments = "";
            ShowStatus = false;
            SendStatus = "";
        }
        public SendBookingEmailModel(int eventId, Guid userId, int seatingId, int numberOfTickets)
        {
            EventId = eventId;
            UserId = userId;
            SeatingId = seatingId;
            NumberOfTickets = numberOfTickets;
            EmailAddress = "";
            Comments = "";
            ShowStatus = false;
            SendStatus = "";
        }

        public int EventId { get; set; }
        public int SeatingId { get; set; }
        public int NumberOfTickets { get; set; }
        public Guid UserId { get; set; }
        public string EmailAddress { get; set; }
        public string Comments { get; set; }
        public bool ShowStatus { get; set; }
        public string SendStatus { get; set; }
    }
}