using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Domain;

namespace SupperClub.Models
{
    public class AllUsersModel
    {
        public IList<User> AllUsers;
    }

    public class AllSupperClubsModel
    {
        public IList<Domain.SupperClub> AllSupperClubs;
    }

    public class NewEventsModel
    {
        public IList<Domain.Event> NewEvents;
    }
    public class AllVouchersModel
    {
        public IList<Domain.Voucher> AllActiveVouchers { get; set; }
        public IList<Domain.Voucher> AllInactiveVouchers { get; set; }        
        public string VoucherCode { get; set; }
    }
    public class Log4NetModel
    {
        public List<Log> LogEvents { get; set; }
        public string FilterText { get; set; }
    }

    public class TestEmailModel
    {
        public int SelectedEmailTemplate { get; set; }
        public string EmailAddress { get; set; }
    }

    public class SendGuestEmail
    {
        public int EventId { get; set; }
        public string UserEmailList { get; set; }
        public bool SendToAll { get; set; }
        public bool ShowStatus { get; set; }
        public string SendStatus { get; set; }
        public string Comments { get; set; }
    }

    public class CMSPopularGrubClubsModel
    {
        public IList<PopularEventAdmin> PopularEvents;
    }

}