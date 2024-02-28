using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeFirstStoredProcs;

namespace SupperClub.Domain
{
    public class PopularEventAdminRank
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int Rank { get; set; }

        public string EventUrl { get; set; }
    }

    public class  PopularEventAdmin 
    {
        public PopularEvent popularEvent { get; set; }
        public int AdminRank { get; set; }
        public string EventUrl { get; set; }
    }

    public class PopularEventRank 
    {

        public int Id { get; set; }
        public int EventId { get; set; }
        public int Rank { get; set; }
        public int Status { get; set; }
        
        public virtual Event Event { get; set; }

        public int AdminRank { get; set; }

        public string EventUrl { get; set; }
    }
    //public class PopularEventsAdmin
    //{
    //    public List<PopularEventAdmin> PopularEventAdmin { get; set; }
    //}

    //public class PopularEventsAdminRank
    //{
    //    public List<PopularEventAdminRank> PopularEventAdminRank { get; set; }
    //}

   
}