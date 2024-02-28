using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class GeoLocation
    {
        public GeoLocation(int distance ,double latitude, double longitude)
        {
            this.Distance = distance;
            this.Latitude = latitude;
            this.Longitude = longitude;
        }
        public int Distance { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
