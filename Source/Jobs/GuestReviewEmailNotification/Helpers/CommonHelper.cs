using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Data;
using GuestReviewEmailNotification.Helpers;

using System.Data.SqlClient;

namespace GuestReviewEmailNotification.Helpers
{
    public enum LogLevel
    {
        Log = 1,
        Error = 2
    }
    public enum NotificationType
    {
        Review = 3 ,
        ReviewReminder = 4
    }
    public static class CommonHelper
    {
        public static string WishListEventInfoBody = "<tr><td valign=\"top\" class=\"mcnBoxedTextBlockInner\"><table align=\"left\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"366\" class=\"mcnBoxedTextContentContainer\"><tbody><tr><td class=\"mcnBoxedTextContentColumn\" style=\"padding-top:9px; padding-left:18px; padding-bottom:9px; padding-right:0;\"><table border=\"0\" cellspacing=\"0\" class=\"mcnTextContentContainer\" width=\"100%\" style=\"border: 1px solid #FFFFFF;background-color: #FFFFFF;\"><tbody><tr><td valign=\"top\" class=\"mcnTextContent\">[eventName],<br>[eventDateTime],<br>[eventCity],<br>[eventCost]</td></tr></tbody></table></td></tr></tbody></table><table align=\"right\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"157\" class=\"mcnBoxedTextContentContainer\"><tbody><tr><td class=\"mcnBoxedTextContentColumn\" style=\"padding-top:9px; padding-right:18px; padding-bottom:9px; padding-left:0;\"><table border=\"0\" cellspacing=\"0\" class=\"mcnTextContentContainer\" width=\"100%\" style=\"border: 1px solid #FFFFFF;background-color: #FFFFFF;\"><tbody><tr><td valign=\"top\" class=\"mcnTextContent\"><div style=\"text-align: left;\"><a href=\"[eventUrl]?[utmParameters]\" style=\"color:#FF8C00\"><span style=\"color:#FF8C00\"><strong><span style=\"font-size:28px\">Book Now!</span></strong></span></a></div></td></tr></tbody></table></td></tr></tbody></table></td></tr>";
        public static string FollowChefInfoBody = "[grubClubName] - [eventName] - <a href=\"[eventUrl]?[utmParameters]\" style=\"color:#FF8C00;text-decoration: none;\"><span style=\"color:#FF8C00\">See Event</span></a><br><br>";

        /// <summary>
        /// Replaces keywords in the template with given values.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="toReplace">The strings to replace. Key is value to replace, Value is replacement value</param>
        /// <returns>Template with changed values.</returns>   
        public static string ReplaceTemplate(string template, Dictionary<string, string> toReplace)
        {
            if (toReplace != null)
            {
                foreach (KeyValuePair<string, string> kvp in toReplace)
                {
                    template = template.Replace("[" + kvp.Key + "]", kvp.Value);
                }
            }
            return template;
        }

        public static void Log(string message, LogLevel ll, Nullable<NotificationType> nt = null, string email = null)
        {
            try
            {
                CommonHelper.InsertEmailNotificationServiceLogDetails((int)ll, (nt== null ? null : (int?)nt), email, message);
             
            }
            catch(Exception ex)
            {
                Configuration.ErrorCounter += 1;
                Configuration.ErrorText += DateTime.Now.ToString() + "\t" + ex.Message + "\t" + ex.StackTrace + "\n";
                try
                {
                string path = "";
                message += Environment.NewLine;
                switch(ll)
                {
                    case LogLevel.Log:                    
                        if (!System.IO.Directory.Exists(Configuration.InfoLogDirectory))
                            System.IO.Directory.CreateDirectory(Configuration.InfoLogDirectory);
                        path = Configuration.InfoLogDirectory + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";
                        break;
                    case LogLevel.Error:
                        if (!System.IO.Directory.Exists(Configuration.ErrorLogDirectory))
                            System.IO.Directory.CreateDirectory(Configuration.ErrorLogDirectory);
                        path = Configuration.ErrorLogDirectory + DateTime.Now.ToShortDateString().Replace("/", "-") + ".txt";
                        break;
                }

                // This text is added only once to the file. 
                if (!File.Exists(path))
                {
                    // Create a file to write to. 
                    File.WriteAllText(path, DateTime.Now.ToString() + "\t" + message);
                }

                // This text is always added
                File.AppendAllText(path, DateTime.Now.ToString() + "\t" + message);
                }
                catch(Exception exc)
                {
                    Configuration.ErrorCounter += 1;
                    Configuration.ErrorText += DateTime.Now.ToString() + "\t" + exc.Message + "\t" + exc.StackTrace + "\n";
                }
            }
        }

        public static string HTMLEncodeSpecialChars(string text)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (!string.IsNullOrEmpty(text))
            {
                foreach (char c in text)
                {
                    if (c > 127) // special chars
                        sb.Append(String.Format("&#{0};", (int)c));
                    else
                        sb.Append(c);
                }
            }
            return sb.ToString();
        }
        private static bool InsertEmailNotificationServiceLogDetails(int LogLevelId, Nullable<int> NotificationTypeId, string Email, string Message)
        {
            bool status = false;
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;

            SqlCommand command = new SqlCommand("InsertEmailNotificationServiceLogDetails", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@LogLevelId", LogLevelId));
            command.Parameters.Add(new SqlParameter("@Message", Message));
            command.Parameters.Add(new SqlParameter("@NotificationTypeId", NotificationTypeId));
            command.Parameters.Add(new SqlParameter("@Email", Email));

            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);

            if (ds != null)
            {
                status = bool.Parse((ds.Tables[0].Rows[0][0].ToString() == "1" ? "true":"false"));
            }
            return status;
        }
    }
}
