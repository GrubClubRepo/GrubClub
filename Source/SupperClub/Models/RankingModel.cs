using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupperClub.Models
{
    public class RankingModel
    {
        public int? EventId { get; set; }
        public bool CurrentUserIsGuest { get; set; }
        public int? Ranking { get; set; }
        public int NumberGuestRanks { get; set; }

        /// <summary>
        /// This model can be used for a SupperClub ranking or Event Ranking.
        /// If the current user has attended the event they will be able to set a ranking
        /// </summary>
        /// <param name="eventId">Optional (null if SupperClub)</param>
        /// <param name="currentUserIsGuest"></param>
        /// <param name="ranking">Out of 5 from the SupperClub model or the Event model</param>
        /// <param name="numberGuestRanks">total from the SupperClub model or the Event model</param>
        public RankingModel(int? eventId, bool currentUserIsGuest, decimal? ranking, int numberGuestRanks)
        {
            EventId = eventId;
            CurrentUserIsGuest = currentUserIsGuest;
            if (ranking == null)
                Ranking = null;
            else
                Ranking = (int)Math.Round((decimal)ranking, 0);
            NumberGuestRanks = numberGuestRanks;
        }
    }

    public class UserRankingModel
    {
        public int? EventId { get; set; }
        public System.Guid UserId { get; set; }
        public SupperClub.Domain.User User { get; set; }
        public int MenuOptionId { get; set; }
        public SupperClub.Domain.EventMenuOption EventMenuOption { get; set; }
        public int NumberOfGuests { get; set; }
        public virtual Nullable<int> UserRanking
        {
            get;
            set;
        }

        public UserRankingModel(int eventId, System.Guid userId, SupperClub.Domain.User user, int menuOptionId, SupperClub.Domain.EventMenuOption eventMenuOption, int numberOfGuests, Nullable<int> userRanking)
        {
            EventId = eventId;
            UserId = userId;
            User = user;
            MenuOptionId = menuOptionId;
            EventMenuOption = eventMenuOption;
            NumberOfGuests = numberOfGuests;
            UserRanking = userRanking;
        }
    }
}