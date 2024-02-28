using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;
using System.Web.Mvc;

namespace SupperClub.Models
{
    public class ReviewList
    {
        public int EventId { get; set; }
        public string EventName { get; set; }
        public string EventUrlFriendlyName { get; set; }
        public DateTime ReviewDate { get; set; }
        public Review EventReview { get; set; }         
    }    
    public class SupperClubReviewModel
    {
        public IList<ReviewList> Reviews { get; set; }
        public SupperClub.Domain.SupperClub SupperClub { get; set; }
        public bool ShowSummary { get; set; }
        public decimal AverageRating 
        {
            
            get
            {
                decimal averageRank = 0;
                decimal totalScore = 0;
                int numberGuestsRanked = 0;

                if (this.Reviews != null)
                {
                    foreach (ReviewList rl in this.Reviews)
                    {
                        if (rl.EventReview != null)
                        {
                            if (rl.EventReview.Rating != null)
                                {
                                    totalScore += (decimal)rl.EventReview.Rating;
                                    numberGuestsRanked++;
                                }
                        }
                    }
                }

                if (numberGuestsRanked > 0)
                {
                    averageRank = Math.Round((totalScore / (decimal)numberGuestsRanked),MidpointRounding.AwayFromZero);                    
                }

                return averageRank;
            }
        }
        public int NumberOfRatings
        {
            get
            {
                int numberGuestRanks = 0;
                if (this.Reviews != null)
                {
                    foreach (ReviewList rl in this.Reviews)
                    {
                        if (rl.EventReview != null && rl.EventReview.Rating != null)
                        {
                            numberGuestRanks++; 
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
                if (this.Reviews != null)
                {
                    foreach (ReviewList rl in this.Reviews)
                    {
                        //if (rl.EventReview != null && rl.EventReview.PublicReview != null && rl.EventReview.PublicReview.Length > 0)
                        if (rl.EventReview != null)
                            numberGuestReviews++;                        
                    }
                }
                return numberGuestReviews;
            }
        }
    }

    public class SupperClubModel
    {
        public SupperClub.Domain.SupperClub SupperClub
        { get; set; }
        public RegisterModel RegisterModel { get; set; }
        public LogOnModel LogOnModel { get; set; }
        public string RedirectURL { get; set; }
        public List<string> Images { get; set; }

    }


    public class PrivateReviewModel
    {

        public IList<Review> Reviews { get; set; }


    }

}