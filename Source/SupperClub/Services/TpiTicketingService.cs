﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using System.Web.Security;
using SupperClub.Models;
using log4net;
using System.Web.Configuration;
using SupperClub.Code;
using SupperClub.WebUI;
using SupperClub.Logger;
using System.Configuration;
using System.Web.SessionState;

namespace SupperClub.Services
{
    public class TpiTicketingService
    {
        private const string CartCookieKey = "BasketId";
        private const string CartUserKey = "tpuid";
        private int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TpiTicketBasketSessionTimeOut"]);
        
        protected static ISupperClubRepository _supperClubRepository;
        protected static readonly ILog log = LogManager.GetLogger(typeof(TicketingService));

        public TpiTicketingService(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        public Guid BasketId { get; set; }
        public Guid UserId { get; set; }

        #region Ticket Basket Management

        public static void ClearBasketSession(HttpContextBase context)
        {
            LogMessage("Clear Basket Cookie", LogLevel.DEBUG);
            if (context.Request.Cookies[CartCookieKey] != null)
            {
                context.Response.Cookies[CartCookieKey].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(context.Response.Cookies[CartCookieKey]);
            }
            if (context.Request.Cookies[CartUserKey] != null)
            {                   
                // Remove Temp UserId from Log table
                Guid tempUserId = Guid.Parse(context.Request.Cookies[CartUserKey].Value);
                bool removeSuccess = _supperClubRepository.RemoveTempUserId(tempUserId);
                if (removeSuccess)
                    LogMessage(string.Format("Temp user id removed successfully. TempUserId: {1}", tempUserId.ToString()), LogLevel.DEBUG);
                context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(context.Response.Cookies[CartUserKey]);
            }
          
        }

        public bool TicketBasketAlive(HttpContextBase context)
        {
            if (context.Request.Cookies[CartCookieKey] == null)
            {
                LogMessage("Basket Dead! (no cookie)", LogLevel.WARN);
                return false;
            }
            else
            {
                string basketId = context.Request.Cookies[CartCookieKey].Value;
                context.Response.Cookies[CartCookieKey].Expires = DateTime.Now.AddDays(-1);
                context.Response.Cookies.Add(context.Response.Cookies[CartCookieKey]);

                context.Response.Cookies[CartCookieKey].Value = basketId;
                context.Response.Cookies[CartCookieKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);
                context.Response.Cookies.Add(context.Response.Cookies[CartCookieKey]);

                TicketBasket ticketBasket = GetTicketBasket(context);
                string tempUserId = string.Empty;                

                if(!UserMethods.IsLoggedIn)
                {
                    string userId = string.Empty;
                    if (context.Request.Cookies[CartUserKey] != null)
                    {
                        //Set Temp User cookie
                        userId = context.Request.Cookies[CartUserKey].Value;
                        context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddDays(-1);
                        context.Response.Cookies.Add(context.Response.Cookies[CartUserKey]);

                        context.Response.Cookies[CartUserKey].Value = userId;
                        context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);
                        context.Response.Cookies.Add(context.Response.Cookies[CartUserKey]);
                    }
                    else
                    {
                        // Create a temp UserId
                        Guid _tempUserId = Guid.NewGuid();
                        // check to see if this userid already exists
                        while (_supperClubRepository.IsExistUserId(_tempUserId) != false)
                        {
                            _tempUserId = Guid.NewGuid();
                        }
                        userId = _tempUserId.ToString();
                        // Store the UserId in Temp User Id log
                        bool storeSuccess = _supperClubRepository.AddTempUserId(_tempUserId);
                        // Set the user cookie
                        context.Response.Cookies[CartUserKey].Value = _tempUserId.ToString();
                        context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);
                        context.Response.Cookies.Add(context.Response.Cookies[CartUserKey]);
                    }

                    if(ticketBasket.UserId.ToString() != userId)
                    {                        
                        ticketBasket.UserId = Guid.Parse(userId);
                        ticketBasket.Name = "Guest";
                    }
                }
                else
                {
                    if (ticketBasket.UserId != (Guid)UserMethods.UserId)
                    {
                        //Assign ticket basket to loggend in user
                        ticketBasket.UserId = (Guid)UserMethods.UserId;
                        ticketBasket.Name = context.User.Identity.Name;    
                        foreach(Ticket t in ticketBasket.Tickets)
                        {
                            t.UserId = ticketBasket.UserId;
                        }
                    }
                    if (context.Request.Cookies[CartUserKey] != null)
                    {
                        //Clean up user cookie
                        tempUserId = context.Request.Cookies[CartUserKey].Value;
                        context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddDays(-1);
                        context.Response.Cookies.Add(context.Response.Cookies[CartUserKey]);
                    }
                }

                if (ticketBasket == null)
                    return false;
                else
                {
                    ticketBasket = UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);
                    LogMessage(string.Format("Basket kept alive. Basket Id: {0}", basketId), LogLevel.DEBUG);
                    // Remove Temp UserId from Log table
                    if(!string.IsNullOrEmpty(tempUserId))
                    {
                        bool removeSuccess = _supperClubRepository.RemoveTempUserId(Guid.Parse(tempUserId));
                        if(removeSuccess)
                            LogMessage(string.Format("Temp user id removed successfully. Basket Id: {0}, TempUserId: {1}, UserId: {2}", basketId, tempUserId.ToString(), UserMethods.UserId.ToString()), LogLevel.DEBUG);
                    }
                    return true;
                }
            }
        }

        public TicketBasket CreateTicketBasketForGiftvoucher(HttpContextBase context, Guid UserId)
        {
            CleanUpAbandonedBaskets();
            BasketId = Guid.NewGuid();
            // UserId = (Guid)UserMethods.UserId;

            context.Response.Cookies[CartCookieKey].Value = BasketId.ToString();
            context.Response.Cookies[CartCookieKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);

            TicketBasket basket = _supperClubRepository.CreateBasket(BasketId, UserId, "Gift Voucher");
            LogMessage(string.Format("Basket Created. Basket Id: {0}", BasketId), LogLevel.DEBUG);
            return basket;
        }
        public TicketBasket CreateTicketBasket(HttpContextBase context)
        {
            CleanUpAbandonedBaskets();
            BasketId = Guid.NewGuid();
            if(UserMethods.IsLoggedIn)
                UserId = (Guid)UserMethods.UserId;
            else
            {
                // Create a temp UserId
                Guid tempUserId = Guid.NewGuid();
                // check to see if this userid already exists
                while (_supperClubRepository.IsExistUserId(tempUserId) != false)
                {
                    tempUserId = Guid.NewGuid();
                }
                UserId = tempUserId;
                // Store the UserId in Temp User Id log
                bool storeSuccess = _supperClubRepository.AddTempUserId(tempUserId);
                // Set the user cookie
                context.Response.Cookies[CartUserKey].Value = tempUserId.ToString();
                context.Response.Cookies[CartUserKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);
            }

            context.Response.Cookies[CartCookieKey].Value = BasketId.ToString();
            context.Response.Cookies[CartCookieKey].Expires = DateTime.Now.AddMinutes(TicketBasketSessionTimeOut);

            TicketBasket basket = _supperClubRepository.CreateBasket(BasketId, UserId, UserMethods.IsLoggedIn ? context.User.Identity.Name : "Guest");
            LogMessage(string.Format("Basket Created. Basket Id: {0}", BasketId), LogLevel.DEBUG);
            return basket;
        }

        public TicketBasket GetTicketBasket(HttpContextBase context)
        {
            TicketBasket basket = null;
           if (context.Request.Cookies[CartCookieKey] != null)
                {
                    Guid basketId = new Guid(context.Request.Cookies[CartCookieKey].Value.ToString());
                    basket = _supperClubRepository.GetBasket(basketId);
                    LogMessage(string.Format("Got Basket. Basket Id: {0}", basketId), LogLevel.DEBUG);
                }
                else
                {
                    LogMessage(string.Format("Could not retrieve Basket! (no cookie)", true), LogLevel.WARN);
                }
            return basket;
        }

        public TicketBasket UpdateTicketBasket(TicketBasket updatedTicketBasket, TicketBasketStatus status)
        {
            TicketBasket tb = _supperClubRepository.UpdateTicketBasket(updatedTicketBasket, status);
            if(tb.UserId != tb.Tickets[0].UserId)
            {
                // Update Tickets
                bool ticketsUpdateStatus = _supperClubRepository.UpdateTicketUser(tb.Id, tb.UserId);
                if(ticketsUpdateStatus)
                    LogMessage(string.Format("Updated ticket user to '{0}'. BasketId: {1}", tb.UserId, tb.Id), LogLevel.DEBUG);
            }
            if (updatedTicketBasket != null)
                LogMessage(string.Format("Updated Ticket Basket to status '{0}'. BasketId: {1}", status, tb.Id), LogLevel.DEBUG);
            else
                LogMessage("Could not Update Ticket Basket! (no basket passed in)", LogLevel.ERROR);

            return tb;
        }

        public void ClearBasket(HttpContextBase context)
        {
            TicketBasket basket = GetTicketBasket(context);
            if (basket != null)
                RemoveTicketsFromBasket(basket.Tickets);
            ClearBasketSession(context);
        }

        public void CleanUpAbandonedBaskets()
        {
            Tuple<int, int> result = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut + 1);
            LogMessage(string.Format("Flushed {0} Expired Baskets ({1} tickets)", result.Item1, result.Item2), LogLevel.DEBUG);
        }

        #endregion

        #region Controller Functions

        public TPIBookingModel GetBookingModelFromBasket(HttpContextBase context)
        {
            TicketBasket basket = GetTicketBasket(context);
            if (basket != null)
            {
                // Private Property used for Adding Tickets
                BasketId = basket.Id;
                UserId = basket.UserId;

                TPIBookingModel booking = new TPIBookingModel();

                if (basket.Tickets.Count > 0)
                {
                    booking.bookingRequirements = basket.BookingRequirements;
                    booking.numberOfTickets = basket.TotalTickets;
                    booking.baseTicketCost = basket.Tickets[0].BasePrice;
                    booking.totalTicketCost = basket.Tickets[0].TotalPrice;
                    booking.totalDue = basket.TotalPrice;
                    booking.eventId = basket.Tickets[0].EventId;
                    booking.seatingId = basket.Tickets[0].SeatingId;
                    booking.voucherId = basket.Tickets[0].VoucherId;
                    booking.discount = basket.TotalDiscount;
                    if (booking.voucherId > 0)
                    {
                        Voucher _voucher = _supperClubRepository.GetVoucher(booking.voucherId);
                        if (_voucher != null)
                            booking.voucherDescription = _voucher.Description;
                    }
                    booking.currency = "GBP";
                    Event _event = _supperClubRepository.GetEvent(booking.eventId);
                    booking.EventName = _event.Name;
                    booking.IsContactNumberRequired = _event.ContactNumberRequired;
                    //booking.contactNumber = (basket.Tickets[0].EventId != 777) ? SupperClub.Code.UserMethods.CurrentUser.ContactNumber : "12345678910";
                    booking.contactNumber =  UserMethods.IsLoggedIn ? SupperClub.Code.UserMethods.CurrentUser.ContactNumber : "";
                    booking.MinMaxBookingEnabled = _event.MinMaxBookingEnabled;
                    booking.SeatSelectionInMultipleOfMin = _event.SeatSelectionInMultipleOfMin;
                    booking.MinTicketsAllowed = _event.MinTicketsAllowed;
                    booking.MaxTicketsAllowed = _event.MaxTicketsAllowed;
                    booking.ToggleMenuSelection = _event.ToggleMenuSelection;
                    booking.commission = _event.Commission;
                    if (_event.MultiMenuOption)
                    {
                        var menuModel = basket.Tickets
                        .GroupBy(u => new { u.MenuOptionId, u.BasePrice, u.DiscountAmount })
                        .Select(lg => new { menuOptionId = lg.Key.MenuOptionId, basePrice = lg.Key.BasePrice, discount = lg.Key.DiscountAmount,  numberOfTickets = lg.Count() }).ToList();

                        if (booking.bookingMenuModel == null)
                            booking.bookingMenuModel = new List<TPIBookingMenuModel>();

                        foreach (EventMenuOption emo in _event.EventMenuOptions)
                        {
                            TPIBookingMenuModel bmm = new TPIBookingMenuModel();
                            bmm.menuOptionId = emo.Id;
                            bmm.menuTitle = emo.Title;
                            bmm.baseTicketCost = emo.Cost;
                            bmm.numberOfTickets = 0;
                            booking.bookingMenuModel.Add(bmm);
                        }
                        for (int i = 0; i < menuModel.Count; i++)
                        {
                            if (menuModel[i].menuOptionId > 0)
                            {
                                foreach (TPIBookingMenuModel bmm in booking.bookingMenuModel)
                                {
                                    if (bmm.menuOptionId == menuModel[i].menuOptionId)
                                    {
                                        bmm.numberOfTickets = menuModel[i].numberOfTickets;
                                        bmm.discount = menuModel[i].discount * menuModel[i].numberOfTickets;
                                    }
                                }
                            }
                        }
                    }

                    if (!_event.MultiSeating)
                    {
                        booking.EventDateAndTime = _event.Start.ToString("ddd, d MMM yyyy") + "  " + _event.Start.ToShortTimeString() + " - " + _event.End.ToShortTimeString();
                    }
                    else
                    {
                        booking.EventDateAndTime = _event.Start.ToString("ddd, d MMM yyyy");
                        if (booking.seatingId > 0)
                        {
                            if (booking.bookingSeatingModel == null)
                                booking.bookingSeatingModel = new List<TPIBookingSeatingModel>();
                            foreach (EventSeating es in _event.EventSeatings)
                            {
                                if (es.AvailableSeats > 0)
                                {
                                    TPIBookingSeatingModel bsm = new TPIBookingSeatingModel();
                                    bsm.seatingId = es.Id;
                                    bsm.start = es.Start;
                                    bsm.availableSeats = es.AvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent(_event.Id, es.Id);
                                    booking.bookingSeatingModel.Add(bsm);
                                }
                            }
                        }
                    }
                    LogMessage(string.Format("Got Booking from Basket Id: {0}", basket.Id), LogLevel.DEBUG);
                }
                else
                {
                    booking.bookingRequirements = basket.BookingRequirements;
                    booking.numberOfTickets = basket.TotalTickets;
                    booking.baseTicketCost = 0;
                    booking.totalTicketCost = 0;
                    booking.totalDue = basket.TotalPrice;
                    booking.eventId = 0;
                    booking.seatingId = 0;
                    booking.discount = 0;
                    booking.currency = "GBP";
                }
                return booking;
            }
            return null;
        }

        public UpdateTicketsResult UpdateBasketTickets(TPIBookingModel booking, int numberTicketsRequested, HttpContextBase context, int currentBlockedTicketsForUser)
        {
            UpdateTicketsResult updateTicketsResult = new UpdateTicketsResult();
            TicketBasket basket = GetTicketBasket(context);

            if (booking != null && basket != null)
            {
                // Private Property used for Adding Tickets
                BasketId = basket.Id;
                UserId = basket.UserId;

                // Available tickets for current user = Total Unallocated Seats for the event + previous booking tickets
                int availableTicketsForCurrentUser = GetCurrentAvailableTicketsForEvent(booking.eventId, booking.seatingId) + currentBlockedTicketsForUser;
                if (numberTicketsRequested > availableTicketsForCurrentUser)
                    updateTicketsResult.Success = false;
                else
                {
                    booking.numberOfTickets = numberTicketsRequested;

                    updateTicketsResult.Success = RemoveTicketsFromBasket(basket.Tickets);
                    
                    if (updateTicketsResult.Success)
                        updateTicketsResult.Success = AddTicketsToBasket(booking, false);

                    // Get the updated basket, or else the old one will be used (no changes)
                    if (updateTicketsResult.Success)
                        basket = _supperClubRepository.GetBasket(BasketId);
                    if(booking.voucherId > 0)
                        updateTicketsResult.DiscountAmount = basket.Tickets.Sum(x => x.DiscountAmount);
                }
                updateTicketsResult.NumberTicketsAllocated = basket.TotalTickets;
                updateTicketsResult.NumberTicketsAvailable = availableTicketsForCurrentUser;

                LogMessage(string.Format("Updated Tickets for Basket Id: {0}. Tickets allocated: {1}", basket.Id, basket.TotalTickets));
            }
            return updateTicketsResult;
        }
 
        #endregion

        #region Ticket Management
        public int GetCurrentAvailableTicketsForEvent(int eventId)
        {
            Event _event = _supperClubRepository.GetEvent(eventId);
            int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(eventId);
            return _event.TotalNumberOfAvailableSeats - ticketsInProgress;
        }
        public int GetCurrentAvailableTicketsForEvent(int eventId, int seatingId)
        {
            if(seatingId == 0)
                return GetCurrentAvailableTicketsForEvent(eventId);
            
            EventSeating _eventSeating = _supperClubRepository.GetEventSeating(seatingId);
            int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(eventId, seatingId);
            return _eventSeating.TotalNumberOfAvailableSeats - ticketsInProgress;
        }
        public bool RemoveTicketsFromBasket(List<Ticket> tickets)
        {
            try
            {
                int numberTicketsToRemove = tickets.Count;
                if (numberTicketsToRemove > 0)
                {
                    TicketBasket basket = _supperClubRepository.RemoveFromBasket(tickets);
                    LogMessage(string.Format("Removed {0} tickets from Basket Id: {1}", numberTicketsToRemove, this.BasketId), LogLevel.DEBUG);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return false;
        }

        //public bool AddGiftVoucherToBasket(BookingModel booking, decimal price,bool newbasket,Guid UserId )
        public bool AddGiftVoucherToBasket(BookingModel booking, decimal price,bool newbasket )
        {
            List<Ticket> tickets = new List<Ticket>();

            decimal baseprice =Convert.ToDecimal( Convert.ToDouble(price) - (Convert.ToDouble(price) * 0.1));

            Ticket ticket = new Ticket(booking.eventId, baseprice, this.BasketId, this.UserId, booking.seatingId, 0, booking.commission);
            ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1}, Menu Option Id: {2})", booking.eventId, booking.seatingId, 0);
            ticket.TotalPrice = price;
            tickets.Add(ticket);

            TicketBasket basket = _supperClubRepository.AddToBasket(tickets);
            return true;
        }
        public bool AddTicketsToBasket(TPIBookingModel booking, bool newbasket)
        {
            try
            {
                if (newbasket)
                {
                    if (GetCurrentAvailableTicketsForEvent(booking.eventId) <= 0)
                        return false;                    
                }
                else
                {
                    if (GetCurrentAvailableTicketsForEvent(booking.eventId, booking.seatingId) <= 0)
                        return false;
                }

                List<Ticket> tickets = new List<Ticket>();
                
                #region Voucher processing
                //Voucher voucher = new Voucher();
                //if(booking.voucherId > 0)
                //    voucher = _supperClubRepository.GetVoucher(booking.voucherId);

                //if (booking != null && voucher != null && voucher.Id > 0)
                //{
                //    if (booking.bookingMenuModel != null && booking.bookingMenuModel.Count > 0)
                //    {
                //        decimal totalDue = 0;
                //        foreach (TPIBookingMenuModel bmm in booking.bookingMenuModel)
                //        {
                //            if (bmm.numberOfTickets > 0)
                //                totalDue = totalDue + (SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission) * bmm.numberOfTickets);
                //        }

                //        foreach (TPIBookingMenuModel bmm in booking.bookingMenuModel)
                //        {
                //            if (bmm.numberOfTickets > 0)
                //            {
                //                decimal discount = 0;
                //                if (voucher.TypeId == (int)VoucherType.PercentageOff)
                //                {
                //                    decimal x = (decimal)voucher.OffValue / 100;
                //                    discount = x * SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission);
                //                }
                //                else if (voucher.TypeId == (int)VoucherType.ValueOff)
                //                {
                //                    discount = ((decimal)voucher.OffValue * SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission)) / totalDue;
                //                }
                //                else if (voucher.TypeId == (int)VoucherType.GiftVoucher)
                //                {
                //                    if (voucher.AvailableBalance >= totalDue)
                //                        voucher.OffValue = totalDue;
                //                    else
                //                        voucher.OffValue = voucher.AvailableBalance;
                //                    discount = ((decimal)voucher.OffValue * SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission)) / totalDue;
                //                }
                //                //else if (voucher.TypeId == (int)VoucherType.PartialPercentOff)
                //                //{
                //                //    discount = discount_pd;
                //                //}
                //                if (discount > SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission))
                //                    discount = SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, booking.commission);
                //                bmm.discount = discount * bmm.numberOfTickets;
                                
                //                for (int i = 0; i < bmm.numberOfTickets; i++)
                //                {
                //                    Ticket ticket = new Ticket(booking.eventId, bmm.baseTicketCost, this.BasketId, this.UserId, booking.seatingId, bmm.menuOptionId, booking.commission, booking.voucherId, discount);
                //                    ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1}, Menu Option Id: {2})", booking.eventId, booking.seatingId, bmm.menuOptionId);
                //                    tickets.Add(ticket);
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        decimal discount = 0;
                //        decimal totalDue = SupperClub.Domain.CostCalculator.CostToGuest(booking.baseTicketCost, booking.commission) * booking.numberOfTickets;
                //        if (voucher.TypeId == (int)VoucherType.PercentageOff)
                //        {
                //            decimal x = (decimal)voucher.OffValue / 100;
                //            discount = x * SupperClub.Domain.CostCalculator.CostToGuest(booking.baseTicketCost, booking.commission);
                //        }
                //        else if (voucher.TypeId == (int)VoucherType.ValueOff)
                //        {
                //            discount = (decimal)voucher.OffValue / booking.numberOfTickets;
                //        }
                //        else if (voucher.TypeId == (int)VoucherType.GiftVoucher)
                //        {
                //            if (voucher.AvailableBalance >= totalDue)
                //                voucher.OffValue = totalDue;
                //            else
                //                voucher.OffValue = voucher.AvailableBalance;
                //            discount = ((decimal)voucher.OffValue) / booking.numberOfTickets;
                //        }
                //        //else if (voucher.TypeId == (int)VoucherType.PartialPercentOff)
                //        //{
                //        //    decimal t = basket.Tickets.OrderBy(b => b.TotalPrice).Take((int)voucher.FreeBooking).ToList().Select(c => c.TotalPrice).Sum();

                //        //    discount = ((decimal)voucher.OffValue * t) / (basket.TotalTickets * 100);
                //        //}
                //        if (discount > SupperClub.Domain.CostCalculator.CostToGuest(booking.baseTicketCost, booking.commission))
                //            discount = SupperClub.Domain.CostCalculator.CostToGuest(booking.baseTicketCost, booking.commission);
                //        for (int i = 0; i < booking.numberOfTickets; i++)
                //        {
                //            Ticket ticket = new Ticket(booking.eventId, booking.baseTicketCost, this.BasketId, this.UserId, booking.seatingId, 0, booking.commission, booking.voucherId, discount);
                //            ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1})", booking.eventId, booking.seatingId);
                //            tickets.Add(ticket);
                //        }
                //    }
                //}
                //else
                //{
                #endregion
                    if (booking.bookingMenuModel == null || booking.bookingMenuModel.Count == 0)
                    {
                        for (int i = 0; i < booking.numberOfTickets; i++)
                        {
                            Ticket ticket = new Ticket(booking.eventId, booking.baseTicketCost, this.BasketId, this.UserId, booking.seatingId,0, booking.commission, booking.voucherId);
                            Event scEvent = _supperClubRepository.GetEvent(booking.eventId);
                            ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1})", scEvent.Id, booking.seatingId);
                            tickets.Add(ticket);
                        }
                    }
                    else
                    {
                        foreach (TPIBookingMenuModel bmm in booking.bookingMenuModel)
                        {
                            for (int i = 0; i < bmm.numberOfTickets; i++)
                            {
                                Ticket ticket = new Ticket(booking.eventId, bmm.baseTicketCost, this.BasketId, this.UserId, booking.seatingId, bmm.menuOptionId, booking.commission);
                                ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1}, Menu Option Id: {2})", booking.eventId, booking.seatingId, bmm.menuOptionId);
                                tickets.Add(ticket);
                            }
                        }
                    }
                //}
                TicketBasket basket = _supperClubRepository.AddToBasket(tickets);
                LogMessage(string.Format("Added {0} tickets to Basket Id: {1}", booking.numberOfTickets, this.BasketId), LogLevel.DEBUG);
                return true;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return false;
        }

        public bool AddUserToEvent(TicketBasket ticketBasket, TPIBookingModel bm)
        {
            bool success = false;
            try
            {
                int eventId = ticketBasket.Tickets[0].EventId;
                int voucherId = ticketBasket.Tickets[0].VoucherId;
                Voucher voucher = null;
                if (voucherId > 0)
                    voucher = _supperClubRepository.GetVoucher(voucherId);

                Event _event = _supperClubRepository.GetEvent(eventId);
                UserId = ticketBasket.UserId;                
                int error = 0;
                if (bm.seatingId > 0)
                {
                    if (bm.bookingMenuModel != null && bm.bookingMenuModel.Count > 0)
                    {
                        foreach (TPIBookingMenuModel bmm in bm.bookingMenuModel)
                        {
                            if (bmm.numberOfTickets > 0)
                            {
                                
                                if (!_supperClubRepository.AddUserToAnEvent(this.UserId, 
                                    eventId, 
                                    bm.seatingId, 
                                    bmm.menuOptionId, 
                                    bmm.numberOfTickets , 
                                    bmm.baseTicketCost*(bmm.numberOfTickets), 
                                    (SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, bm.commission)*bmm.numberOfTickets)-bmm.discount,
                                    (voucher != null ? (voucher.OwnerId == (int)VoucherOwner.Host ? ((bmm.baseTicketCost * bmm.numberOfTickets) - bmm.discount) : (bmm.baseTicketCost * bmm.numberOfTickets)) : (bmm.baseTicketCost * bmm.numberOfTickets)),
                                    SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost,bm.commission)*(bmm.numberOfTickets),
                                    bmm.discount,
                                    voucherId,
                                    ticketBasket.Id,
                                    voucher !=null ? (voucher.OwnerId == (int)VoucherOwner.Admin ? true : false) : false))
                                        error++;
                                
                            }
                        }
                    }
                    else
                    {
                        if (!_supperClubRepository.AddUserToAnEvent(this.UserId, 
                            eventId, 
                            bm.seatingId, 
                            0, 
                            ticketBasket.TotalTickets,
                            bm.baseTicketCost * (ticketBasket.TotalTickets),
                            ((SupperClub.Domain.CostCalculator.CostToGuest(bm.baseTicketCost,bm.commission) * ticketBasket.TotalTickets) - bm.discount),
                            (voucher != null ? (voucher.OwnerId == (int)VoucherOwner.Host ? ((bm.baseTicketCost * ticketBasket.TotalTickets) - bm.discount) : (bm.baseTicketCost * ticketBasket.TotalTickets)) : (bm.baseTicketCost * ticketBasket.TotalTickets)),
                            SupperClub.Domain.CostCalculator.CostToGuest(bm.baseTicketCost,bm.commission) * (ticketBasket.TotalTickets),
                            bm.discount,
                            voucherId,
                            ticketBasket.Id,
                            voucher != null ? (voucher.OwnerId == (int)VoucherOwner.Admin ? true : false) : false))
                                error++;                        
                    }
                }
                else
                {
                    if (bm.bookingMenuModel != null && bm.bookingMenuModel.Count > 0)
                    {
                        foreach (TPIBookingMenuModel bmm in bm.bookingMenuModel)
                        {
                            if (bmm.numberOfTickets > 0)
                            {
                                if (!_supperClubRepository.AddUserToAnEvent(this.UserId, 
                                    eventId, 
                                    0, 
                                    bmm.menuOptionId, 
                                    bmm.numberOfTickets, 
                                    bmm.baseTicketCost*(bmm.numberOfTickets), 
                                    ((SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost,bm.commission)*bmm.numberOfTickets)-bmm.discount),
                                    (voucher != null ? (voucher.OwnerId == (int)VoucherOwner.Host ? ((bmm.baseTicketCost*bmm.numberOfTickets)-bmm.discount):(bmm.baseTicketCost*bmm.numberOfTickets)):(bmm.baseTicketCost*bmm.numberOfTickets)),
                                    SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost,bm.commission)*(bmm.numberOfTickets),
                                    bmm.discount,
                                    voucherId,
                                    ticketBasket.Id,
                                    voucher !=null ? (voucher.OwnerId == (int)VoucherOwner.Admin ? true : false) : false))
                                        error++;                                
                            }
                        }
                    }
                    else
                    {
                        if (!_supperClubRepository.AddUserToAnEvent(this.UserId, 
                            eventId, 
                            0, 
                            0, 
                            ticketBasket.TotalTickets,
                            bm.baseTicketCost*(ticketBasket.TotalTickets),                                
                            ((SupperClub.Domain.CostCalculator.CostToGuest(bm.baseTicketCost,bm.commission)*ticketBasket.TotalTickets)-bm.discount),
                            (voucher != null ? (voucher.OwnerId == (int)VoucherOwner.Host ? ((bm.baseTicketCost * ticketBasket.TotalTickets) - bm.discount): (bm.baseTicketCost * ticketBasket.TotalTickets)) : (bm.baseTicketCost * ticketBasket.TotalTickets)),
                            SupperClub.Domain.CostCalculator.CostToGuest(bm.baseTicketCost,bm.commission)*(ticketBasket.TotalTickets),
                            bm.discount,
                            voucherId,
                            ticketBasket.Id,
                            voucher !=null ? (voucher.OwnerId == (int)VoucherOwner.Admin ? true : false) : false))
                                error++;
                    }
                }
                if (error == 0)
                    success = true;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return success;
        }
        #endregion

        #region Logging

        private static void LogMessage(string message, LogLevel level = LogLevel.INFO)
        {
            WebUILogging.LogMessage(message, log, level);
        }

        private static void LogException(string message, Exception ex)
        {
            WebUILogging.LogException(message, ex, log);
        }

        #endregion
    }
}