using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using SupperClub.Logger;
namespace SupperClub.Domain.Repository
{
    public interface ISupperClubRepository
    {

        #region Events

        Event GetEvent(int eventId);
        IList<EventWaitList> GetEventWaitList(int eventId);
        Event GetFirstActiveEvent();

        Event GetEventByName(string url); 
        
        EventSeating GetEventSeating(int seatingId);

        EventMenuOption GetEventMenuOption(int menuOptionId);
        
        IList<Event> GetAllEvents();

        IList<Event> GetAllActiveEvents();

        IList<Event> GetAllApprovedEvents();
        IList<SiteMapEventList> GetAllSEOEventUrls();
        IList<Review> GetSupperClubReviews(int eventId, int supperClubId);
        IList<Event> GetRecentlyAddedEvents();

        IList<Event> GetPastEvents(DateTime eventsAfter);

        IList<Event> GetPastEventsForASupperClub(int supperClubId);

        IList<Event> GetFutureEventsForASupperClub(int supperClubId);
        
        IList<Event> GetAllEventsForASupperClub(int supperClubId);

        IList<Event> GetAllActiveEventsForASupperClub(int supperClubId);

        Event GetNextEventForASupperClub(int supperClubId);

        IList<Event> GetPastEventsForAUser(Guid userId);

        IList<Event> GetFutureEventsForAUser(Guid userId);

        IList<MyBookedEvent> GetUserPastEvents(Guid userId);

        IList<MyBookedEvent> GetUserFutureEvents(Guid userId);

        IList<Event> GetAllEventsForAUser(Guid userId);

        Event GetNextEventForAUser(Guid userId);

        EventSeating GetEventSeatingForAUser(int eventId, Guid userId);

        IList<Event> GetNewlyCreatedEvents();
        IList<EventMenuOption> GetEventMenuOptionForAUser(int eventId, Guid userId);

        IList<string> GetUnusedImageList(int eventId, List<string> imagesToBeDeletedEventList);

        Event CreateEvent(Event _event);

        bool UpdateEvent(Event _event, Guid userId);

        bool CancelEvent(int eventId);

        bool UpdateEventSeating(EventSeating _eventSeating);

        bool UpdateEventMenuOption(EventMenuOption _eventMenuOption);

        bool AddUserToAnEvent(Guid userId, int eventId, int seatingId, int menuOptionId, int numberOfGuests, decimal totalBasePrice, decimal totalPrice, decimal? hostNetPrice = null, decimal? GuestBasePrice = null, decimal discount = 0, int? voucherId = 0, Guid? basketId = null, bool? AdminVoucherCode = null);

        bool RemoveUserFromAnEvent(Guid userId, int eventId, int seatingId, int menuOptionId, Guid adminUserId);
        
        bool RemoveUserFromAnEvent(Guid userId, int eventId);

        bool RemoveUserFromAnEvent(Guid userId, int eventId, int seatingId);

        bool RankEvent(int eventId, Guid eventAttendeeId, int ranking);

        int AddTag(string tagName, string tagSeoName);

        bool UpdateEventStatus(Event _event);
        //bool AddEventToNewlyCreatedEvent(int eventId);
        IList<TileTag> GetTileTags();

        IList<PressRelease> GetPressReleases();

        IList<Event> GetWaitlistedEventsForAUser(Guid userId);

        IList<Event> GetFavouriteEventsForAUser(Guid userId);
        IList<int> GetFavouriteEventIdsForAUser(Guid userId);
        bool IsCurrentEventUsersFavourite(int eventId, Guid userId);

        bool IsCurrentSupperClubUsersFavourite(int supperClubId, Guid userId);

        Event GetFavouriteEvent();

        Tag GetTagByName(string url);
        City GetCityByName(string url);
        Area GetAreaByName(string url);

        bool LogEventPriceChange(EventPriceChangeLog _eventPriceChangeLog);
        bool LogEventCommissionChange(EventCommissionChangeLog _eventCommissionChangeLog);
        int CloneEvent(int EventId, DateTime Start, DateTime End, decimal Commission);
        #endregion

        #region SupperClubs

        int GetSupperClubFollowers(int supperClubId);

        IList<User> GetSupperClubFollowersList(int supperClubId);

        IList<SupperClub> GetAllSupperClubs();

        IList<SupperClub> GetAllActiveSupperClubs();

        IList<SupperClubs> GetAllSupperClubDetails(string Keyword);

        IList<SupperClubs> GetAllSupperClubDetails(Guid? UserId);

        IList<SupperClub> GetAllInactiveSupperClubs();

        SupperClub GetSupperClub(int supperClubId);

        SupperClub GetSupperClub(string hostname);

        SupperClub GetSupperClubForUser(string email);

        IList<Event> GetAllEventsWithReview(string hostname);

        SupperClub GetSupperClubForUser(Guid userId);

        SupperClub RegisterSupperClub(SupperClub supperClub, string email);

        bool UpdateSupperClub(SupperClub supperClub);

        bool UpdateSupperClubProfileTags(int Id, string ProfileTags);

        bool? FlipActivationForSupperClub(int supperClubId);

        int IsExistSupperClub(string supperClubName, Guid userId);

        IList<SupperClub> GetUserFavouriteSupperClubs(Guid userId);

        int GetUserFavouriteSupperClubCount(Guid userId);
        #endregion

        #region User

        bool CreateUser(User user);

        bool AddUserDevice(UserDevice userDevice);

        bool CheckUserDevice(string deviceId, Guid userId);

        User GetUser(Guid? userId);

        User GetUserNoTracking(Guid userId);

        IList<User> GetAllUsers();

        bool RankUser(int eventId, Guid eventAttendeeId, int ranking);
        
        /// <summary>
        /// Flips whether a user is locked out or not
        /// </summary>
        /// <param name="userId">userId Guid</param>
        /// <returns>new state of lock (true locked out, false not locked out, null if flip fails)</returns>
        bool? FlipUserLock(Guid userId);

        User GetUserByFbId(string fbId);

        bool UpdateUser(User user);

        bool AddEventsToFavourite(List<UserFavouriteEvent> userFaavouriteEvents);

        bool AddEventToFavourite(UserFavouriteEvent userFaavouriteEvent);

        bool RemoveEventsFromFavourite(int[] eventId, Guid userId);

        bool RemoveEventFromFavourite(int eventId, Guid userId);
        List<User> GetWishListedUsers(int eventId);        
        bool AddSupperClubsToFavourite(List<UserFavouriteSupperClub> userFavouriteSupperClubs);

        bool AddSupperClubToFavourite(UserFavouriteSupperClub userFavouriteSupperClub);

        bool RemoveSupperClubsFromFavourite(int[] supperClubId, Guid userId);

        bool RemoveSupperClubFromFavourite(int supperClubId, Guid userId);

        bool AddUserFacebookFriend(List<UserFacebookFriend> userFacebookFriend);

        UserDevice GetUserDevice(string deviceToken);

        bool UpdateNotificationState(UserDevice userDevice);

        bool IsExistUserId(Guid tempUserId);

        bool IsExistSegmentUserId(Guid tempUserId);
        SegmentUser GetSegmentUser(Guid userId);
        bool AddTempUserId(Guid tempUserId);

        SegmentUser AddSegmentUser(SegmentUser _segmentUser);

        bool RemoveTempUserId(Guid tempUserId);
        #endregion
        
        #region Search
        IList<SearchResult> SearchEvent(Search search, SearchFilter filter);
        IList<EventListingResult> GetEvents(EventListing eventListing, EventListingFilter filter, bool searchAll);
        IList<EventListResult> SearchEvents(EventList eventList);
        IList<PriceRange> GetPriceRange();
        PriceRange GetMinMaxPriceRange(int[] priceRangeIds);
        IList<Diet> GetDiets();
        IList<Diet> GetStandardDiets();
        IList<Diet> GetAllergyDiets();
        IList<Diet> GetOtherAllergyDiets(int eventId);
        List<int> GetAllAllergyDiets();
        IList<Tag> GetTags();
        IList<ProfileTag> GetProfileTags();
        string GetSupperClubProfileTags(int supperClubId);
        IList<Tag> GetTagsWithoutCuisine();
        IList<Tag> GetActiveEventTags();
        IList<City> GetCities();
        IList<Area> GetAreas();
        IList<Cuisine> GetCuisines();
        IList<Cuisine> GetAvailableCuisines(int tagId);
        IList<SearchCategory> GetSearchCategories();
        IList<Tag> GetSearchCategoryTags(string categoryName);
        SearchCategory GetSearchCategoryByName(string name);
        SearchCategoryTag GetSearchCategoryTagByTagId(int tagId);
        IList<SearchCategory> GetAllSearchCategories();
        IList<SiteMapCityCategoryTagList> GetAllSearchCategoryTagCityList();
        IList<SiteMapCityAreaCategoryTagList> GetAllSearchCategoryTagCityAreaList();
        IList<Tag> GetAllTagsWithoutCategory();
        IList<PopularEvent> GetAllActivePopularEvents();
        IList<PopularEvent> GetActivePopularEventsForEventPage(int eventId);
        IList<City> GetAllCities();
        IList<Area> GetAllAreas();
        bool CheckTagCategoryCombinationValidity(string categoryname, string tagname);

        #endregion

        #region Tickets and Basket

        TicketBasket CreateBasket(Guid basketId, Guid userId, string name);
        bool CleanUpBaskets(Guid BasketId);
        TicketBasket GetBasket(Guid id);
        TicketBasket GetExistingBasket(Guid id);
        TicketBasket UpdateTicketBasket(TicketBasket ticketBasket, TicketBasketStatus status);
        
        TicketBasket AddToBasket(List<Ticket> tickets);
        TicketBasket RemoveFromBasket(List<Ticket> tickets);
        bool CheckBookingForBasket(Guid basketId);

        int GetNumberTicketsInProgressForEvent(int eventId);
        int GetNumberTicketsInProgressForEvent(int eventId, int seatingId);
        int GetNumberTicketsInProgressForUser(int eventId, int seatingId, Guid userId);
        int GetPreviousEventTicketsForUser(Guid userId, int eventId, int seatingId, int menuOptionId);
        int GetPreviousEventTicketsForUser(Guid userId, int eventId, int seatingId);
        
        Tuple<int, int> CleanUpAbandonedBaskets(int olderThanMinutes);

        bool CheckInUserToEvent(int bookingReferenceNum, Guid userId);
        bool ResetCheckInToEvent(int bookingReferenceNum, Guid userId);

        int CheckVoucherCode(string voucherCode, int eventId, Guid userId, int totalBookings, decimal basketValue);
        Voucher GetVoucher(int voucherId);
        Voucher GetVoucherDetail(int voucherId);
        Voucher GetVoucher(string voucherCode);
        bool UpdateVoucher(Voucher voucher);

        IList<Diet> GetEventDiets(int eventId);

        string GetBookingRequirements(int eventId, Guid userId);
        bool UpdateTicketUser(Guid basketId, Guid userId);
        #endregion

        #region Transactions

        PaymentTransaction CreateTransction(PaymentTransaction transaction);
        PaymentTransaction GetPaymentTransaction(string VendorTxCode);

        PayPalTransaction CreateTransction(PayPalTransaction transaction);
        BraintreeTransaction CreateTransction(BraintreeTransaction transaction);
        #endregion

        #region Misc
        bool CreateBrainTreeCustomer(BraintreeCustomer customer);
        BraintreeCustomer GetBraintreeCustomer(Guid UserId);
        EmailTemplate GetEmailTemplate(int Id);
        IList<PopularEventAdmin> GetPopularEventDetailsWithAdminRanks();
        MessageTemplate GetMessageTemplate(int templateId);
        GeoLocation GetGeoLocation(string postcode, string city, string address = null, string address2 = null);
        List<Log> GetLog(int days, LogLevel logLevel, string filter);

        Log GetLogEvent(int logId);

        bool PurgeLog(int olderThanDays);

        List<Report> GetReportList(int reportTypeEnumId);

        Report GetReport(int reportId);

        List<User> GetUser(List<string> userEmailList);

        User GetUser(string email);

        DataTable RunReportQuery(string sql, List<Tuple<string, string>> parameters);

        bool AddSubscriber(Subscriber subscriber);

        int AddToWaitList(EventWaitList eventWaitList);

        bool RemoveFromWaitList(int eventId, Guid userId);

        bool IsUserAddedToWaitList(Guid userId, int eventId);

        bool AddGuestName(string email, string name);

        string GetGuestName(string email);

        bool AddGuestEmailInfo(Guid currentUser, List<Subscriber> lstSubscriber);

        UrlRewrite GetUrl(string urlRewrite);

        List<UrlRewrite> GetUrlRewrites();

        bool AddPushNotificationLog(PushNotificationLog _pushNotificationLog);
        List<string> GetFacebookFriendListForPushNotification(Guid userId, string faceBookId);

        List<string> GetFacebookFriendListForBookingPushNotification(Guid userId, string faceBookId);        

        List<string> GetUserListForNewEventPushNotification(int supperClubId);

        List<string> GetUserListForEventBookingReminderPushNotification(int supperClubId);

        List<string> GetUserListForEventWaitListPushNotification(int supperClubId);
        
        bool UpdateUsersPushNotificationForEventBookingReminder(int eventId);

        bool UpdateUsersEmailNotificationForEventBookingReminder(int eventId, Guid userId);

        int ValidateTagName(string tagName, string tagSeoName, int tagId = 0);
        int ValidateProfileTagName(string tagName, int tagId = 0);
        bool CreateTag(Tag model);

        bool CreateProfileTag(ProfileTag model);
        
        bool UpdateTag(Tag model);

        bool UpdateProfileTag(ProfileTag model);

        Tag GetTag(int tagId);

        ProfileTag GetProfileTag(int tagId);
        SearchCategory GetSearchCategory(int searchCategoryId);

        int ValidateCategoryName(string categoryName, string categorySeoName, int categoryId = 0);

        bool CreateCategory(SearchCategory model);

        bool UpdateCategory(SearchCategory model);

        
        Voucher CreateVoucher(Voucher model);

        bool CheckVoucherCode(string voucherCode);

        IList<Voucher> GetAllActiveVouchers();

        IList<Voucher> GetAllInactiveVouchers();

        bool ChangeVoucherStatus(int voucherId, bool newStatus);

        bool DeactivateVoucher(string voucherCode);

        UserVoucherTypeDetail CreateUserVoucherTypeDetail(UserVoucherTypeDetail userVoucherTypeDetail);

         SearchLogDetail CreateSeacrchLogDetail(SearchLogDetail searchLogDetail);

          bool IsUserValidToRefer(User user);

          bool IsUserValidByRefer(string email);

          bool IsValidUser(Guid userId);

          SupperclubStatusChangeLog AddSupperclubStatusChangeLog(SupperclubStatusChangeLog supperclubStatusChangeLog);


          BookingConfirmationToFriends CreateBookingConfiramtionToFriends(BookingConfirmationToFriends bookingConfirmationToFriends);

          bool ValidateEventUrlFriendlyName(string urlFriendlyName);

          IList<Event> GetActiveFutureEvents(string urlFriendlyName);

          Event GetLastActivePastEvent(string urlFriendlyName);
          IList<EventAttendee> GetRecentBookingDetailsForUser(Guid userId);

          bool UpdateImageUrl(ImageType imageType, string oldImageUrl, string newImageUrl);
        #endregion

        #region Reviews

        Review GetReview(int reviewId);

        Review GetReview(int eventId, Guid userId);

        List<Review> GetPublishedReviewsForEvent(int eventId);

        List<Review> GetAllReviewsForEvent(int eventId);

        bool AddReview(Review review);

        bool UpdateReview(Review review);

        bool? FlipPublishOrUnpublish(int reviewId);

        bool DeleteReview(int reviewId);

        #endregion

        #region CMS

        PageCMS AddUpdatePageCMS(PageCMS pageCMS);

        IList<PageCMS> GetCMSDetailsByPage(string pageName);

        bool UpdatePageCMS(PageCMS pageCMS);

        int GetAdminRankByEventId(int EventId);

        string GetAdminUrlByEventId(int EventId);

        IList<PopularEventAdmin> GetPopularEventsWithAdminRank();
        
        void UpdatePopularEventAdminRank(IList<PopularEventAdminRank> PopularEventsAdmin);

        PopularEventAdminRank CreatePopularEventAdminRank(PopularEventAdminRank popularEventsAdmin);

        #endregion

    }
}
