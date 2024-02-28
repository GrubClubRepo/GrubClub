using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using GuestReviewEmailNotification.Helpers;
using GuestReviewEmailNotification.BLL;

namespace GuestReviewEmailNotification.DLL
{
    public class GetUserEventInfo
    {
        
        #region Public Methods

        public List<UserEventInfo> GetEventReviewEmailUserList(int offset)
        {
            List<UserEventInfo> _UserEventInfo = new List<UserEventInfo>();
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("GetEventReviewEmailUserList", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Offset", offset));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);            

            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    _UserEventInfo = PopulateEventUserListDetails(dt);                
            }
            return _UserEventInfo;
        }

        public List<UserEventInfo> GetReviewReminderEmailUserList(int reminderCount)
        {
            List<UserEventInfo> _populateEventDetails = new List<UserEventInfo>();
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("GetReviewReminderEmailUserList", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@ReminderCount", reminderCount));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);
            if (ds != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null)
                    _populateEventDetails = PopulateReviewReminderUserListDetails(dt);
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

        private List<UserEventInfo> PopulateEventUserListDetails(DataTable dt)
        {
            List<UserEventInfo> _list = new List<UserEventInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                UserEventInfo _UserEventInfo = new UserEventInfo();
                _UserEventInfo.User = new User();
                if(string.IsNullOrEmpty(dr["UserId"].ToString()))
                    _UserEventInfo.User.UserId = null;
                else
                    _UserEventInfo.User.UserId = Guid.Parse(dr["UserId"].ToString());
                _UserEventInfo.User.FirstName = dr["FirstName"].ToString();
                _UserEventInfo.User.EmailAddress = dr["Email"].ToString();
                _UserEventInfo.EventId = Int32.Parse(dr["EventId"].ToString());
                _UserEventInfo.Name = dr["Name"].ToString();
                _UserEventInfo.UrlFriendlyName = dr["UrlFriendlyName"].ToString();
                _UserEventInfo.SupperClubUrlFriendlyName = dr["SupperClubUrlFriendlyName"].ToString();
                _UserEventInfo.IsFriend = bool.Parse(dr["IsFriend"].ToString()); 
                _list.Add(_UserEventInfo);
            }
            return _list;    
        }
        private List<UserEventInfo> PopulateReviewReminderUserListDetails(DataTable dt)
        {
            List<UserEventInfo> _list = new List<UserEventInfo>();
            foreach (DataRow dr in dt.Rows)
            {
                UserEventInfo _UserEventInfo = new UserEventInfo();
                _UserEventInfo.User = new User();
                if (string.IsNullOrEmpty(dr["UserId"].ToString()))
                    _UserEventInfo.User.UserId = null;
                else
                    _UserEventInfo.User.UserId = Guid.Parse(dr["UserId"].ToString());
                _UserEventInfo.User.FirstName = dr["FirstName"].ToString();
                _UserEventInfo.User.EmailAddress = dr["Email"].ToString();
                _UserEventInfo.EventId = Int32.Parse(dr["EventId"].ToString());
                _UserEventInfo.Name = dr["Name"].ToString();
                _UserEventInfo.UrlFriendlyName = dr["UrlFriendlyName"].ToString();
                _UserEventInfo.SupperClubUrlFriendlyName = dr["SupperClubUrlFriendlyName"].ToString();
                _UserEventInfo.IsFriend = bool.Parse(dr["IsFriend"].ToString());
                _UserEventInfo.RowId = Int32.Parse(dr["Id"].ToString());
                _list.Add(_UserEventInfo);
            }
            return _list;
        }    
        #endregion
    }
    
}