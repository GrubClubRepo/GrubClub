using System;
using System.Web.Configuration;

namespace SupperClub.Domain
{
    public class DateConverter
    {
        public static DateTime GetUTCTime(DateTime localDate)
        {
            // convert date time to UTC
            return TimeZoneInfo.ConvertTimeToUtc(localDate, TimeZoneInfo.Local);
        }
    }
}
