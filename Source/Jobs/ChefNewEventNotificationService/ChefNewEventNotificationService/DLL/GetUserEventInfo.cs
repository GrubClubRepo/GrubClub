using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using ChefNewEventNotificationService.Helpers;
using ChefNewEventNotificationService.BLL;

namespace ChefNewEventNotificationService.DLL
{
    public class GetUserEventInfo
    {
        
        #region Public Methods

        public List<UserWishListEventInfo> GetUserWishListedEventDetails(int offset)
        {
            List<UserWishListEventInfo> _userWishListEventInfo = new List<UserWishListEventInfo>();
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("GetUserEventListForUpcomingWishListedEvents", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Offset", offset));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);            

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    _userWishListEventInfo = PopulateUserWishListEventDetails(dt);                
            }
            return _userWishListEventInfo;
        }

        public List<UserFollowChefEventInfo> UserFollowChefEventDetails()
        {
            List<UserFollowChefEventInfo> _populateEventDetails = new List<UserFollowChefEventInfo>();
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("GetUserEventListForFollowedChefNewEvents", con);
            command.CommandType = CommandType.StoredProcedure;
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    _populateEventDetails = PopulateFollowChefEventDetails(dt);
            }
            return _populateEventDetails;
        }

        public bool UpdateNewEventStatus(string eventIds)
        {
            bool status = false;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("UpdateNewEventStatus", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@EventIds", eventIds));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);

            if (ds != null)
            {
                status = bool.Parse(ds.Tables[0].Rows[0][0].ToString() == "1" ? "true":"false");
            }
            return status;
        }
        


        #endregion

        #region Private Methods - Reader

        private List<UserWishListEventInfo> PopulateUserWishListEventDetails(DataTable dt)
        {
            List<UserWishListEventInfo> _list = new List<UserWishListEventInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                UserWishListEventInfo _userWishListEventInfo = new UserWishListEventInfo();
                _userWishListEventInfo.User = new User();
                _userWishListEventInfo.Event = new Event();
                _userWishListEventInfo.User.UserId = Guid.Parse(dr["UserId"].ToString());
                _userWishListEventInfo.User.FirstName = dr["FirstName"].ToString();
                _userWishListEventInfo.User.EmailAddress = dr["Email"].ToString();
                _userWishListEventInfo.Event.EventId = Int32.Parse(dr["EventId"].ToString());
                _userWishListEventInfo.Event.Name = dr["EventName"].ToString();
                _userWishListEventInfo.Event.Cost = Decimal.Parse(dr["Cost"].ToString());
                _userWishListEventInfo.Event.Commission = Decimal.Parse(dr["Commission"].ToString());
                _userWishListEventInfo.Event.Start = DateTime.Parse(dr["Start"].ToString());
                _userWishListEventInfo.Event.End = DateTime.Parse(dr["End"].ToString());
                _userWishListEventInfo.Event.City = dr["City"].ToString();
                _userWishListEventInfo.Event.UrlFriendlyName = dr["UrlFriendlyName"].ToString();
                _userWishListEventInfo.Event.SupperClubUrlFriendlyName = dr["SupperClubUrlFriendlyName"].ToString();
                _list.Add(_userWishListEventInfo);
            }
            return _list;    
        }


        private List<UserFollowChefEventInfo> PopulateFollowChefEventDetails(DataTable dt)
        {
            List<UserFollowChefEventInfo> _list = new List<UserFollowChefEventInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                UserFollowChefEventInfo _userFollowChefEventInfo = new UserFollowChefEventInfo();
                _userFollowChefEventInfo.User = new User();

                _userFollowChefEventInfo.User.UserId = Guid.Parse(dr["UserId"].ToString());
                _userFollowChefEventInfo.User.FirstName = dr["FirstName"].ToString();
                _userFollowChefEventInfo.User.EmailAddress = dr["Email"].ToString();
                _userFollowChefEventInfo.EventId = Int32.Parse(dr["EventId"].ToString());
                _userFollowChefEventInfo.Name = dr["Name"].ToString();
                _userFollowChefEventInfo.UrlFriendlyName = dr["UrlFriendlyName"].ToString();
                _userFollowChefEventInfo.SupperClubUrlFriendlyName = dr["SupperClubUrlFriendlyName"].ToString();
                _userFollowChefEventInfo.SupperClubName = dr["SupperClubName"].ToString();
                _list.Add(_userFollowChefEventInfo);
            }
            return _list;            
        }

        #endregion
    }
    
}