using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;

namespace SupperClub.Models
{
    public class SearchViewModel
    {
        public IList<SearchResult> SearchResult { get; set; }
        public SearchModel SearchCriteria { get; set; }
        public Tag SearchTag { get; set; }

        public RegisterModel RegisterModel { get; set; }
        public LogOnModel LogOnModel { get; set; }
        public string RedirectURL { get; set; }
    }    
}