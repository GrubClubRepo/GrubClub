using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using SupperClub.Domain;

namespace SupperClub.Configuration
{
    
    public class BookingCancellationLogConfiguration : EntityTypeConfiguration<BookingCancellationLog>
    {
        internal BookingCancellationLogConfiguration()
        {
            this.ToTable("BookingCancellationLog");
            this.HasKey(key => new { key.Id });
        }
    }
    public class CuisineConfiguration : EntityTypeConfiguration<Cuisine>
    {
        internal CuisineConfiguration()
        {
            this.ToTable("Cuisine");
            this.HasKey(key => new { key.Id });
            this.HasMany(b => b.EventCuisines);
        }
    }
    public class VoucherConfiguration : EntityTypeConfiguration<Voucher>
    {
        internal VoucherConfiguration()
        {
            this.ToTable("Voucher");
            this.HasKey(key => new { key.Id });
            this.HasMany(b => b.EventVouchers);
        }
    }
    public class EmailTemplateConfiguration : EntityTypeConfiguration<EmailTemplate>
    {
        internal EmailTemplateConfiguration()
        {
            this.ToTable("EmailTemplate");
            this.HasKey(key => new { key.Id });
        }
    }
    public class MessageTemplateConfiguration : EntityTypeConfiguration<MessageTemplate>
    {
        internal MessageTemplateConfiguration()
        {
            this.ToTable("MessageTemplate");
            this.HasKey(key => new { key.Id });
        }
    }

    public class EventConfiguration : EntityTypeConfiguration<Event>
    {
        internal EventConfiguration()
        {
            this.ToTable("Event");
            this.HasKey(key => new { key.Id });
        }
    }

    public class EventAttendeeConfiguration : EntityTypeConfiguration<EventAttendee>
    {
        internal EventAttendeeConfiguration()
        {
            this.ToTable("EventAttendee");
            this.HasKey(key => new { key.Id });
        }
    }

    public class EventSeatingConfiguration : EntityTypeConfiguration<EventSeating>
    {
        internal EventSeatingConfiguration()
        {
            this.ToTable("EventSeating");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventWaitListConfiguration : EntityTypeConfiguration<EventWaitList>
    {
        internal EventWaitListConfiguration()
        {
            this.ToTable("EventWaitList");
            this.HasKey(key => new { key.Id });
        }
    }
    
    public class EventMenuOptionConfiguration : EntityTypeConfiguration<EventMenuOption>
    {
        internal EventMenuOptionConfiguration()
        {
            this.ToTable("EventMenuOption");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventCuisineConfiguration : EntityTypeConfiguration<EventCuisine>
    {
        internal EventCuisineConfiguration()
        {
            this.ToTable("EventCuisine");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventVoucherConfiguration : EntityTypeConfiguration<EventVoucher>
    {
        internal EventVoucherConfiguration()
        {
            this.ToTable("EventVoucher");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventTagConfiguration : EntityTypeConfiguration<EventTag>
    {
        internal EventTagConfiguration()
        {
            this.ToTable("EventTag");
            this.HasKey(key => new { key.Id });
        }
    }

    public class SupperClubProfileTagsConfiguration : EntityTypeConfiguration<SupperClubProfileTag>
    {
        internal SupperClubProfileTagsConfiguration()
        {
            this.ToTable("SupperClubProfileTag");
            this.HasKey(key => new { key.Id });
        }

    }

    
    public class EventRecommendationConfiguration : EntityTypeConfiguration<EventRecommendation>
    {
        internal EventRecommendationConfiguration()
        {
            this.ToTable("EventRecommendation");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventImageConfiguration : EntityTypeConfiguration<EventImage>
    {
        internal EventImageConfiguration()
        {
            this.ToTable("EventImage");
            this.HasKey(key => new { key.Id });
        }
    }
    public class FavouriteEventConfiguration : EntityTypeConfiguration<FavouriteEvent>
    {
        internal FavouriteEventConfiguration()
        {
            this.ToTable("FavouriteEvent");
            this.HasKey(key => new { key.Id });
        }
    }    
    public class SupperClubConfiguration : EntityTypeConfiguration<SupperClub.Domain.SupperClub>
    {
        internal SupperClubConfiguration()
        {
            this.ToTable("SupperClub");
            this.HasKey(key => new { key.Id });

        }
    }
    public class SupperClubImageConfiguration : EntityTypeConfiguration<SupperClubImage>
    {
        internal SupperClubImageConfiguration()
        {
            this.ToTable("SupperClubImage");
            this.HasKey(key => new { key.Id });
        }
    }
    public class SupperClubVoucherConfiguration : EntityTypeConfiguration<SupperClubVoucher>
    {
        internal SupperClubVoucherConfiguration()
        {
            this.ToTable("SupperClubVoucher");
            this.HasKey(key => new { key.Id });
        }
    }
    public class UsageEventTypeConfiguration : EntityTypeConfiguration<UsageEventType>
    {
        internal UsageEventTypeConfiguration()
        {
            this.ToTable("UsageEventType");
            this.HasKey(key => new { key.ID });
        }
    }

    public class UserConfiguration : EntityTypeConfiguration<User>
    {
        internal UserConfiguration()
        {
            this.ToTable("User");
            this.HasKey(key => new { key.Id });
        }
    }

    public class UserInviteeConfiguration : EntityTypeConfiguration<UserInvitee>
    {
        internal UserInviteeConfiguration()
        {
            this.ToTable("UserInvitee");
            this.HasKey(key => new { key.Id });
        }
    }

    public class UserDeviceConfiguration : EntityTypeConfiguration<UserDevice>
    {
        internal UserDeviceConfiguration()
        {
            this.ToTable("UserDevice");
            this.HasKey(key => new { key.Id });
        }
    }
     
    public class UserFavouriteEventConfiguration : EntityTypeConfiguration<UserFavouriteEvent>
    {
        internal UserFavouriteEventConfiguration()
        {
            this.ToTable("UserFavouriteEvent");
            this.HasKey(key => new { key.Id });
        }
    }
    public class UserFavouriteSupperClubConfiguration : EntityTypeConfiguration<UserFavouriteSupperClub>
    {
        internal UserFavouriteSupperClubConfiguration()
        {
            this.ToTable("UserFavouriteSupperClub");
            this.HasKey(key => new { key.Id });
        }
    }
    public class UserFacebookFriendConfiguration : EntityTypeConfiguration<UserFacebookFriend>
    {
        internal UserFacebookFriendConfiguration()
        {
            this.ToTable("UserFacebookFriend");
            this.HasKey(key => new { key.Id });
        }
    }
    public class aspnet_UserConfiguration : EntityTypeConfiguration<aspnet_Users>
    {
        internal aspnet_UserConfiguration()
        {
            this.ToTable("aspnet_Users");
            this.HasKey(key => new { key.UserId });
        }
    }

    public class aspnet_MembershipConfiguration : EntityTypeConfiguration<aspnet_Membership>
    {
        internal aspnet_MembershipConfiguration()
        {
            this.ToTable("aspnet_Membership");
            this.HasKey(key => new { key.UserId });
        }
    }

    public class UserSearchConfiguration : EntityTypeConfiguration<UserSearch>
    {
        internal UserSearchConfiguration()
        {
            this.ToTable("UserSearch");
            this.HasKey(key => new { key.SearchTerms });
        }
    }

    public class UserUsageStatisticConfiguration : EntityTypeConfiguration<UserUsageStatistic>
    {
        internal UserUsageStatisticConfiguration()
        {
            this.ToTable("UserUsageStatistic");
            this.HasKey(key => new { key.ID });
        }
    }
    public class SegmentUserConfiguration : EntityTypeConfiguration<SegmentUser>
    {
        internal SegmentUserConfiguration()
        {
            this.ToTable("SegmentUser");
            this.HasKey(key => new { key.SegmentUserId });
        }
    }
    
    public class MenuConfiguration : EntityTypeConfiguration<Menu>
    {
        internal MenuConfiguration()
        {
            this.ToTable("Menu");
            this.HasKey(key => new { key.Id });
        }
    }

    public class PriceRangeConfiguration : EntityTypeConfiguration<PriceRange>
    {
        internal PriceRangeConfiguration()
        {
            this.ToTable("PriceRange");
            this.HasKey(key => new { key.Id });
        }
    }

    public class DietConfiguration : EntityTypeConfiguration<Diet>
    {
        internal DietConfiguration()
        {
            this.ToTable("Diet");
            this.HasKey(key => new { key.Id });
            this.HasMany(d => d.EventDiets);
        }
    }
    public class TagConfiguration : EntityTypeConfiguration<Tag>
    {
        internal TagConfiguration()
        {
            this.ToTable("Tag");
            this.HasKey(key => new { key.Id });
            this.HasMany(d => d.EventTags);
        }
    }

    public class ProfileTagConfiguration : EntityTypeConfiguration<ProfileTag>
    {
        internal ProfileTagConfiguration()
        {
            this.ToTable("ProfileTag");
            this.HasKey(key => new { key.Id });
            this.HasMany(d => d.SupperClubTags);
        }
    }
    public class TempUserIdLogConfiguration : EntityTypeConfiguration<TempUserIdLog>
    {
        internal TempUserIdLogConfiguration()
        {
            this.ToTable("TempUserIdLog");
            this.HasKey(key => new { key.Id });
        }
    }
    public class TileTagConfiguration : EntityTypeConfiguration<TileTag>
    {
        internal TileTagConfiguration()
        {
            this.ToTable("TileTag");
            this.HasKey(key => new { key.TileId, key.TagId, key.ImageId });
        }
    }
    public class PushNotificationLogConfiguration : EntityTypeConfiguration<PushNotificationLog>
    {
        internal PushNotificationLogConfiguration()
        {
            this.ToTable("PushNotificationLog");
            this.HasKey(key => new { key.Id });
        }
    }
    public class UserFavouriteSupperClubEventNotificationConfiguration : EntityTypeConfiguration<UserFavouriteSupperClubEventNotification>
    {
        internal UserFavouriteSupperClubEventNotificationConfiguration()
        {
            this.ToTable("UserFavouriteSupperClubEventNotificationp");
            this.HasKey(key => new { key.Id });
        }
    }
    public class TileConfiguration : EntityTypeConfiguration<Tile>
    {
        internal TileConfiguration()
        {
            this.ToTable("Tile");
            this.HasKey(key => new { key.Id });
        }
    }
    public class SearchCategoryConfiguration : EntityTypeConfiguration<SearchCategory>
    {
        internal SearchCategoryConfiguration()
        {
            this.ToTable("SearchCategory");
            this.HasKey(key => new { key.Id });
        }
    }
    public class SearchCategoryTagConfiguration : EntityTypeConfiguration<SearchCategoryTag>
    {
        internal SearchCategoryTagConfiguration()
        {
            this.ToTable("SearchCategoryTag");
            this.HasKey(key => new { key.SearchCategoryId, key.TagId });
        }
    }
    public class ImageConfiguration : EntityTypeConfiguration<Image>
    {
        internal ImageConfiguration()
        {
            this.ToTable("Image");
            this.HasKey(key => new { key.Id });
        }
    }    
    public class CityConfiguration : EntityTypeConfiguration<City>
    {
        internal CityConfiguration()
        {
            this.ToTable("City");
            this.HasKey(key => new { key.Id });
            this.HasMany(d => d.EventCities);
        }
    }
    public class AreaConfiguration : EntityTypeConfiguration<Area>
    {
        internal AreaConfiguration()
        {
            this.ToTable("Area");
            this.HasKey(key => new { key.Id });
            this.HasMany(d => d.EventAreas);
        }
    }           
    public class PressReleaseConfiguration : EntityTypeConfiguration<PressRelease>
    {
        internal PressReleaseConfiguration()
        {
            this.ToTable("PressRelease");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventDietConfiguration : EntityTypeConfiguration<EventDiet>
    {
        internal EventDietConfiguration()
        {
            this.ToTable("EventDiet");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventCityConfiguration : EntityTypeConfiguration<EventCity>
    {
        internal EventCityConfiguration()
        {
            this.ToTable("EventCity");
            this.HasKey(key => new { key.Id });
        }
    }
    public class EventAreaConfiguration : EntityTypeConfiguration<EventArea>
    {
        internal EventAreaConfiguration()
        {
            this.ToTable("EventArea");
            this.HasKey(key => new { key.Id });
        }
    }
    public class LogConfiguration : EntityTypeConfiguration<Log>
    {
        internal LogConfiguration()
        {
            this.ToTable("Log");
            this.HasKey(key => new { key.ID });
        }
    }

    public class EventPriceChangeLogConfiguration : EntityTypeConfiguration<EventPriceChangeLog>
    {
        internal EventPriceChangeLogConfiguration()
        {
            this.ToTable("EventPriceChangeLog");
            this.HasKey(key => new { key.ID });
        }
    }

    public class EventCommissionChangeLogConfiguration : EntityTypeConfiguration<EventCommissionChangeLog>
    {
        internal EventCommissionChangeLogConfiguration()
        {
            this.ToTable("EventCommissionChangeLog");
            this.HasKey(key => new { key.Id });
        }
    }
    public class TicketConfiguration : EntityTypeConfiguration<Ticket>
    {
        internal TicketConfiguration()
        {
            this.ToTable("Ticket");
            this.HasKey(key => new { key.Id });
        }
    }

    public class TicketBasketConfiguration : EntityTypeConfiguration<TicketBasket>
    {
        internal TicketBasketConfiguration()
        {
            this.ToTable("TicketBasket");
            this.HasKey(key => new { key.Id });
        }
    }

    public class PaymentTransactionConfiguration : EntityTypeConfiguration<PaymentTransaction>
    {
        internal PaymentTransactionConfiguration()
        {
            this.ToTable("PaymentTransaction");
            this.HasKey(key => new { key.Id });
        }
    }
    
    public class BrainTreeTransactionConfiguration : EntityTypeConfiguration<BraintreeTransaction>
    {
        internal BrainTreeTransactionConfiguration()
        {
            this.ToTable("BrainTreeTransaction");
            this.HasKey(key => new { key.Id });
        }
    }
     public class BrainTreeCustomerConfiguration : EntityTypeConfiguration<BraintreeCustomer>
    {
         internal BrainTreeCustomerConfiguration()
        {
            this.ToTable("BraintreeCustomer");
            this.HasKey(key => new { key.UserId });
        }
    }
    
    public class PayPalTransactionConfiguration : EntityTypeConfiguration<PayPalTransaction>
    {
        internal PayPalTransactionConfiguration()
        {
            this.ToTable("PayPalTransaction");
            this.HasKey(key => new { key.Id });
        }
    }

    public class ReportConfiguration : EntityTypeConfiguration<Report>
    {
        internal ReportConfiguration()
        {
            this.ToTable("Report");
            this.HasKey(key => new { key.Id });
        }
    }

    public class SubscriberConfiguration : EntityTypeConfiguration<Subscriber>
    {
        internal SubscriberConfiguration()
        {
            this.ToTable("Subscriber");
            this.HasKey(key => new { key.Id });
        }
    }

    public class UrlRewriteConfiguration : EntityTypeConfiguration<UrlRewrite>
    {
        internal UrlRewriteConfiguration()
        {
            this.ToTable("UrlRewrite");
            this.HasKey(key => new { key.Id });
        }
    }

    public class ReviewConfiguration : EntityTypeConfiguration<Review>
    {
        internal ReviewConfiguration()
        {
            this.ToTable("Review");
            this.HasKey(key => new { key.Id });
        }
    }
    
    public class PopularEventConfiguration : EntityTypeConfiguration<PopularEvent>
    {
        internal PopularEventConfiguration()
        {
            this.ToTable("PopularEvent");
            this.HasKey(key => new { key.Id });
        }
    }
    public class UserVoucherTypeDetailConfiguration : EntityTypeConfiguration<UserVoucherTypeDetail>
    {
        internal UserVoucherTypeDetailConfiguration()
        {
            this.ToTable("UserVoucherTypeDetail");
            this.HasKey(key => new { key.Id });
        }
    }

    public class SearchLogDetailConfiguration : EntityTypeConfiguration<SearchLogDetail>
    {
        internal SearchLogDetailConfiguration()
        {
            this.ToTable("SearchLogDetail");
            this.HasKey(key => new { key.Id });
        }
    }
    
    public class PageCMSConfiguration : EntityTypeConfiguration<PageCMS>
    {
        internal PageCMSConfiguration()
        {
            this.ToTable("PageCMS");
            this.HasKey(key => new { key.Id });
        }
    }
    public class BookingConfirmationToFriendsConfiguration : EntityTypeConfiguration<BookingConfirmationToFriends>
    {
         internal BookingConfirmationToFriendsConfiguration()
        {
            this.ToTable("BookingConfirmationToFriends");
            this.HasKey(key => new { key.Id });
        }
    }

    public class PopularEventAdminRankConfiguration : EntityTypeConfiguration<PopularEventAdminRank>
    {
        internal PopularEventAdminRankConfiguration()
        {
            this.ToTable("PopularEventAdminRank");
            this.HasKey(key => new { key.Id });
        }
    }

    public class SupperclubStatusChangeLogConfiguration : EntityTypeConfiguration<SupperclubStatusChangeLog>
    {
        internal SupperclubStatusChangeLogConfiguration()
        {
            this.ToTable("SupperclubStatusChangeLog");
            this.HasKey(key => new { key.Id });
        }
    }


    
}