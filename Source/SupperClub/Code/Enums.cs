using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace SupperClub.WebUI
{
    public enum Gender
    {
        Male,
        Female,
        ItsComplicated,
        PreferNotToSay
    }

    public enum Sort
    {
        [Description("Distance")]
        Distance,
        [Description("Recommended")]
        Rating,
        [Description("Highest Price")]
        HightestPrice,
        [Description("Lowest Price")]
        LowestPrice
    }

    public enum ReportType
    {
        GrubClubs = 2,
        Events = 1,
        Users = 3,
        Revenue = 4,
        Misc = 5,
        SagePay = 6,
    }
    public enum PixelTagType
    {
        AppNexusRegistration = 1,
        AppNexusNewsletter = 2        
    }
}