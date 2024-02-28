using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using SupperClub.Domain;
using SupperClub.Configuration;
using System.ComponentModel.DataAnnotations.Schema;


namespace SupperClub.Data.EntityFramework
{
    public class SupperClubContext : DbContext
    {
        public SupperClubContext(): base()
        {
        }

        public DbSet<BookingCancellationLog> BookingCancellationLogs { get; set; }
        public DbSet<Cuisine> Cuisines { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProfileTag> ProfileTags { get; set; }

        public DbSet<TileTag> TileTags { get; set; }
        public DbSet<Tile> Tiles { get; set; }
        public DbSet<TempUserIdLog> TempUserIdLogs { get; set; }
        public DbSet<PushNotificationLog> PushNotificationLogs { get; set; }

        public DbSet<UserFavouriteSupperClubEventNotification> UserFavouriteSupperClubEventNotifications { get; set; }
        public DbSet<SearchCategory> SearchCategories { get; set; }
        public DbSet<SearchCategoryTag> SearchCategoryTags { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Area> Areas { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<PressRelease> PressReleases { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<MessageTemplate> MessageTemplates { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventAttendee> EventAttendees { get; set; }
        public DbSet<EventCuisine> EventCuisines { get; set; }
        public DbSet<EventMenuOption> EventMenuOptions { get; set; }
        public DbSet<EventSeating> EventSeatings { get; set; }
        public DbSet<EventTag> EventTags { get; set; }
        public DbSet<SupperClubProfileTag> SupperClubProfileTags { get; set; }
        public DbSet<EventRecommendation> EventRecommendations { get; set; }
        public DbSet<EventVoucher> EventVouchers { get; set; }
        public DbSet<EventImage> EventImages { get; set; }
        public DbSet<EventCity> EventCities { get; set; }
        public DbSet<EventArea> EventAreas { get; set; }
        public DbSet<FavouriteEvent> FavouriteEvents { get; set; }
        public DbSet<SupperClubImage> SupperClubImages { get; set; }
        public DbSet<SupperClubVoucher> SupperClubVouchers { get; set; }
        public DbSet<SupperClub.Domain.SupperClub> SupperClubs { get; set; }
        public DbSet<UsageEventType> UsageEventTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserInvitee> UserInvitees { get; set; }
        public DbSet<UserDevice> UserDevices { get; set; }
        public DbSet<UserFavouriteEvent> UserFavouriteEvents { get; set; }
        public DbSet<UserFavouriteSupperClub> UserFavouriteSupperClubs { get; set; }
        public DbSet<UserFacebookFriend> UserFacebookFriends { get; set; }
        public DbSet<SegmentUser> SegmentUsers { get; set; }
        public DbSet<aspnet_Users> aspnet_Users { get; set; }
        public DbSet<aspnet_Membership> aspnet_Memberships { get; set; }
        public DbSet<UserSearch> UserSearchs { get; set; }
        public DbSet<UserUsageStatistic> UserUsageStatistics { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<PriceRange> PriceRanges { get; set; }
        public DbSet<Diet> Diets { get; set; }
        public DbSet<EventDiet> EventDiets { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<EventPriceChangeLog> EventPriceChangeLogs { get; set; }
        public DbSet<EventCommissionChangeLog> EventCommissionChangeLogs { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketBasket> TicketBaskets { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
        public DbSet<PayPalTransaction> PayPalTransactions { get; set; }
        public DbSet<BraintreeTransaction> BrainTreeTransactions { get; set; }
        public DbSet<BraintreeCustomer> BrainTreeCustomers { get; set; }
        public DbSet<Subscriber> Subscribers { get; set; }
        public DbSet<EventWaitList> EventWaitLists { get; set; }
        public DbSet<UrlRewrite> UrlRewrites { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<PopularEvent> PopularEvents { get; set; }
        public DbSet<UserVoucherTypeDetail> UserVoucherTypeDetails { get; set; }

        public DbSet<SearchLogDetail> SearchLogDetails { get; set; }

        public DbSet<PageCMS> PageCMS{get; set;}

        public DbSet<BookingConfirmationToFriends> BookingConfirmationToFriends { get; set; }

        public DbSet<PopularEventAdminRank> PopularEventAdminRank { get; set; }
        public DbSet<SupperclubStatusChangeLog> SupperclubStatusChangeLog { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.
            Database.SetInitializer<SupperClubContext>(null);

            //modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // Add the required model binding configurations
            modelBuilder.Configurations
                .Add(new BookingCancellationLogConfiguration())
                .Add(new CuisineConfiguration())
                .Add(new DietConfiguration())
                .Add(new TagConfiguration())
                .Add(new ProfileTagConfiguration())
                .Add(new TempUserIdLogConfiguration())
                .Add(new TileConfiguration())
                .Add(new TileTagConfiguration())
                .Add(new PushNotificationLogConfiguration())
                .Add(new UserFavouriteSupperClubEventNotificationConfiguration())
                .Add(new ImageConfiguration())
                .Add(new CityConfiguration())
                .Add(new AreaConfiguration())
                .Add(new VoucherConfiguration())
                .Add(new PressReleaseConfiguration())
                .Add(new EmailTemplateConfiguration())
                .Add(new MessageTemplateConfiguration())
                .Add(new SearchCategoryConfiguration())
                .Add(new SearchCategoryTagConfiguration())
                .Add(new EventConfiguration())
                .Add(new EventAttendeeConfiguration())
                .Add(new EventCuisineConfiguration())
                .Add(new EventVoucherConfiguration())
                .Add(new EventWaitListConfiguration())
                .Add(new EventSeatingConfiguration())
                .Add(new EventMenuOptionConfiguration())
                .Add(new EventTagConfiguration())
                .Add(new EventRecommendationConfiguration())
                .Add(new EventImageConfiguration())
                .Add(new EventCityConfiguration())
                .Add(new EventAreaConfiguration())
                .Add(new FavouriteEventConfiguration())
                .Add(new SupperClubConfiguration())
                .Add(new SupperClubImageConfiguration())
                .Add(new SupperClubVoucherConfiguration())
                .Add(new SupperClubProfileTagsConfiguration())
                .Add(new UserSearchConfiguration())
                .Add(new UsageEventTypeConfiguration())
                .Add(new UserConfiguration())
                .Add(new UserInviteeConfiguration())
                .Add(new UserDeviceConfiguration())
                .Add(new UserFavouriteEventConfiguration())
                .Add(new UserFavouriteSupperClubConfiguration())
                .Add(new UserFacebookFriendConfiguration())
                .Add(new SegmentUserConfiguration())
                .Add(new UserUsageStatisticConfiguration())
                .Add(new MenuConfiguration())
                .Add(new PriceRangeConfiguration())             
                .Add(new EventDietConfiguration())
                .Add(new aspnet_UserConfiguration())
                .Add(new aspnet_MembershipConfiguration())
                .Add(new LogConfiguration())
                .Add(new EventPriceChangeLogConfiguration())
                .Add(new EventCommissionChangeLogConfiguration())
                .Add(new ReportConfiguration())
                .Add(new TicketConfiguration())
                .Add(new TicketBasketConfiguration())
                .Add(new PaymentTransactionConfiguration())
                .Add(new BrainTreeTransactionConfiguration())
                .Add(new BrainTreeCustomerConfiguration())
                .Add(new PayPalTransactionConfiguration())
                .Add(new SubscriberConfiguration())
                .Add(new UrlRewriteConfiguration())
                .Add(new ReviewConfiguration())
                .Add(new PopularEventConfiguration())
                .Add(new UserVoucherTypeDetailConfiguration())
                .Add(new SearchLogDetailConfiguration())
                .Add(new PageCMSConfiguration())
                .Add(new BookingConfirmationToFriendsConfiguration())
                .Add(new PopularEventAdminRankConfiguration())
                .Add(new SupperclubStatusChangeLogConfiguration())
                ;
            // Additional Properties
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.FutureEvents);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.PastEvents);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.AverageRank);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.NumberGuestRanks);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.File);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.ImagePaths);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().Ignore(s => s.SupperClubProfileTagList);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().HasMany(e => e.Events).WithOptional().HasForeignKey(e => e.SupperClubId);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().HasMany(sci => sci.SupperClubImages).WithOptional().HasForeignKey(sci => sci.SupperClubId);
            modelBuilder.Entity<SupperClub.Domain.SupperClub>().HasMany(sci => sci.SupperClubVouchers).WithOptional().HasForeignKey(sci => sci.SupperClubId);

            modelBuilder.Entity<Event>().Ignore(e => e.Cuisines);
            modelBuilder.Entity<Event>().Ignore(e => e.Menu);
            modelBuilder.Entity<Event>().Ignore(e => e.Diets);
            modelBuilder.Entity<Event>().Ignore(e => e.EventTagList);
            modelBuilder.Entity<Event>().Ignore(e => e.EventCityList);
            modelBuilder.Entity<Event>().Ignore(e => e.EventAreaList);
            modelBuilder.Entity<Event>().Ignore(e => e.SelectedTagIds);
            modelBuilder.Entity<Event>().Ignore(e => e.SelectedDietIds);
            modelBuilder.Entity<Event>().Ignore(e => e.SelectedCityIds);
            modelBuilder.Entity<Event>().Ignore(e => e.SelectedAreaIds);
            modelBuilder.Entity<Event>().Ignore(e => e.OtherAllergyText);
            modelBuilder.Entity<Event>().Ignore(e => e.Seatings);
            modelBuilder.Entity<Event>().Ignore(e => e.MenuOptions);
            modelBuilder.Entity<Event>().Ignore(e => e.ImagePaths);
            modelBuilder.Entity<Event>().Ignore(e => e.TotalNumberOfAttendeeGuests);
            modelBuilder.Entity<Event>().Ignore(e => e.StartTime);
            modelBuilder.Entity<Event>().Ignore(e => e.Duration);
            modelBuilder.Entity<Event>().Ignore(e => e.AverageRank);
            modelBuilder.Entity<Event>().Ignore(e => e.NumberGuestRanks);
            modelBuilder.Entity<Event>().Ignore(e => e.TotalNumberOfAvailableSeats);
            modelBuilder.Entity<Event>().Ignore(e => e.CostToGuest);
            modelBuilder.Entity<Event>().Ignore(e => e.CostTextDisplay);
            modelBuilder.Entity<Event>().Ignore(e => e.File);
            modelBuilder.Entity<Event>().Ignore(e => e.NameDateTime);
            modelBuilder.Entity<Event>().Ignore(e => e.DefaultMenuOptionId);
            modelBuilder.Entity<Event>().Ignore(e => e.DefaultSeatingId);
            modelBuilder.Entity<Event>().Ignore(e => e.TotalEventGuests);
            modelBuilder.Entity<Event>().Ignore(e => e.TotalEventReservedSeats);
            modelBuilder.Entity<Event>().Ignore(e => e.MinSeatingTime);
            modelBuilder.Entity<Event>().Ignore(e => e.NonHtmlDescription);
            modelBuilder.Entity<Event>().Ignore(e => e.ShortDescription);
            modelBuilder.Entity<Event>().Ignore(e => e.TPIAvailableSeats);
            modelBuilder.Entity<Event>().Ignore(e => e.EventDescription);

            modelBuilder.Entity<Event>().Property(e => e.Cost).HasColumnType("Money").HasPrecision(15, 4);

            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventSeatings).WithOptional().HasForeignKey(es => es.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventMenuOptions).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventTags).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventCities).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventAreas).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventRecommendations).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventImages).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventWaitLists).WithOptional().HasForeignKey(em => em.EventId);
            modelBuilder.Entity<SupperClub.Domain.Event>().HasMany(e => e.EventVouchers).WithOptional().HasForeignKey(em => em.EventId);

            modelBuilder.Entity<EventSeating>().Ignore(es => es.TotalNumberOfAttendeeGuests);
            modelBuilder.Entity<EventSeating>().Ignore(es => es.TotalNumberOfAvailableSeats);
            modelBuilder.Entity<EventSeating>().Ignore(es => es.AvailableSeats);

            modelBuilder.Entity<TicketBasket>().Property(tb => tb.BookingReference).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<TicketBasket>().Ignore(tb => tb.TotalPrice);
            modelBuilder.Entity<TicketBasket>().Ignore(tb => tb.TotalTickets);
            modelBuilder.Entity<TicketBasket>().Ignore(tb => tb.TotalDiscount);
            modelBuilder.Entity<TicketBasket>().HasMany(tb => tb.Tickets).WithOptional().HasForeignKey(t => t.BasketId);
            modelBuilder.Entity<TicketBasket>().Ignore(tb => tb.Commission);
            modelBuilder.Entity<Ticket>().Property(t => t.CommissionMultiplier).HasPrecision(18, 4);
            modelBuilder.Entity<Ticket>().Property(t => t.CommissionFixed).HasPrecision(18, 4);
            modelBuilder.Entity<Ticket>().Property(t => t.CommissionTotal).HasPrecision(18, 4);
            modelBuilder.Entity<Ticket>().Property(t => t.VATMultiplier).HasPrecision(18, 4);
            modelBuilder.Entity<Ticket>().Property(t => t.VATTotal).HasPrecision(18, 4);
            modelBuilder.Entity<Ticket>().Property(t => t.DiscountAmount).HasPrecision(18, 4);

            modelBuilder.Entity<EventAttendee>().Property(ea => ea.Discount).HasPrecision(18, 4);

            modelBuilder.Entity<aspnet_Users>().HasRequired(p => p.aspnet_Membership);
            modelBuilder.Entity<Tag>().Ignore(t => t.CostToGuest);
            modelBuilder.Entity<ProfileTag>().Ignore(t => t.CreatedDate);
            
            modelBuilder.Entity<User>().HasRequired(u => u.aspnet_Users);
            modelBuilder.Entity<User>().HasMany(u => u.TicketBaskets).WithOptional().HasForeignKey(tb => tb.UserId);
            modelBuilder.Entity<User>().HasMany(u => u.Tickets).WithOptional().HasForeignKey(t => t.UserId);
            //modelBuilder.Entity<User>().HasMany(u => u.UserInvitees).WithOptional().HasForeignKey(t => t.UserId);
            modelBuilder.Entity<User>().Ignore(u => u.NumberRankedEvents);
            modelBuilder.Entity<User>().Ignore(u => u.PositiveRanking);
            modelBuilder.Entity<User>().Ignore(u => u.NegativeRanking);
            modelBuilder.Entity<User>().Ignore(u => u.UserEvents);
            modelBuilder.Entity<User>().Ignore(u => u.FutureEvents);
            modelBuilder.Entity<User>().Ignore(u => u.PastEvents);
            modelBuilder.Entity<User>().Ignore(u => u.WishListedEventIds);
            modelBuilder.Entity<User>().Ignore(u => u.Locked);
            modelBuilder.Entity<User>().Ignore(u => u.Email);

            modelBuilder.Entity<SearchCategory>().Ignore(e => e.TagList);
            modelBuilder.Entity<SearchCategory>().Ignore(e => e.SelectedTagIds);

            modelBuilder.Entity<Voucher>().Ignore(v => v.EventList);
            modelBuilder.Entity<Voucher>().Ignore(v => v.SupperClubList);
            modelBuilder.Entity<Voucher>().Ignore(v => v.ApplyToAll);

            //modelBuilder.Ignore<YourOtherModelHere>();
            modelBuilder.Ignore<CostCalculator>();
            base.OnModelCreating(modelBuilder);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);            
        }

        
    }
}
