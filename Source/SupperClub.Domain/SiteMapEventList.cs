using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class SiteMapEventList
    {
        public string EventUrlFriendlyName { get; set; }
        public string SupperClubUrlFriendlyName { get; set; }
    }

    public class SiteMapCityCategoryTagList
    {
        public string CityUrlFriendlyName { get; set; }
        public string TagUrlFriendlyName { get; set; }
        public string CategoryUrlFriendlyName { get; set; }
    }
    public class SiteMapCityAreaCategoryTagList
    {
        public string AreaUrlFriendlyName { get; set; }
        public string CityUrlFriendlyName { get; set; }
        public string TagUrlFriendlyName { get; set; }
        public string CategoryUrlFriendlyName { get; set; }
    }
}
