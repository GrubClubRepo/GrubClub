using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Net.Configuration;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using GuestReviewEmailNotification.BLL;

namespace GuestReviewEmailNotification.Helpers
{

    public class EmailHelper
    {
        private bool emailSentStatus = false;

        #region Actually send the emails

        /// <summary>
        /// Sends e-mail message to a single recipient.
        /// </summary>        
        /// <param name="toAddress">Recipient address.</param>
        /// <param name="subject">E-mail subject.</param>
        /// <param name="body">E-mail body.</param>
        /// <param name="isHtml">Message is html or not.</param>        
        /// <returns>True if no error occured.</returns>
        public bool SendMail(string toAddress, string subject, string body, bool isHtml)
        {
            try
            {

               /* if (!(toAddress.ToLower().Contains("hotmail") || toAddress.ToLower().Contains("outlook") ||
                   toAddress.ToLower().Contains("live") || toAddress.ToLower().Contains("msn") ||
                   toAddress.ToLower().Contains("passport")) || toAddress.ToLower().Contains("grubclub"))
                {                    
                    SmtpSection smtpSection = (SmtpSection)(ConfigurationManager.GetSection("mailSettings/smtp_1"));

                    SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                    smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    smtpClient.EnableSsl = false;

                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.From = new MailAddress(smtpSection.From);
                    msg.To.Add(toAddress);
                    smtpClient.Send(msg);
                    smtpClient.Dispose();
                    return true;
                }
                else
                {
                    SmtpSection smtpSection = (SmtpSection)(ConfigurationManager.GetSection("mailSettings/smtp_2"));

                    SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                    smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    smtpClient.EnableSsl = true;

                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.From = new MailAddress(smtpSection.From);
                    msg.To.Add(toAddress);
                    smtpClient.Send(msg);
                    smtpClient.Dispose();
                    return true;

                }*/
                SmtpSection smtpSection = (SmtpSection)(ConfigurationManager.GetSection("mailSettings/smtp_1"));

                SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                smtpClient.EnableSsl = true;

                MailMessage msg = new MailMessage();
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = isHtml;
                msg.BodyEncoding = System.Text.Encoding.ASCII;
                msg.From = new MailAddress(smtpSection.From);
                msg.To.Add(toAddress);
                smtpClient.Send(msg);
                smtpClient.Dispose();
                return true;

            }
            catch (Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
                return false;
            }
        }

        /// <summary>
        /// Sends e-mail message to multiple BCC recipients.
        /// </summary>
        /// <param name="toAddresses">The list of BCC recipients addresses.</param>
        /// <param name="subject">E-mail subject.</param>
        /// <param name="body">E-mail body.</param>
        /// <param name="isHtml">Message is html or not.</param>    
        /// <returns>True if no error occured.</returns>
        public bool SendMailToMany(string toAddress, List<string> bccAddresses, string subject, string body, bool isHtml)
        {
            try
            {
                List<String> lstmailid = new List<string>();
                List<string> lsthotmailid = new List<string>();

                lsthotmailid = bccAddresses.Where(mail => (mail.ToLower().Contains("hotmail")) || (mail.ToLower().Contains("outlook")) || (mail.ToLower().Contains("live")) || (mail.ToLower().Contains("msn")) || (mail.ToLower().Contains("passport")) || (mail.ToLower().Contains("grubclub"))).ToList();
                lstmailid = bccAddresses.Where(mail => !((mail.ToLower().Contains("hotmail")) || (mail.ToLower().Contains("outlook")) || (mail.ToLower().Contains("live")) || (mail.ToLower().Contains("msn")) || (mail.ToLower().Contains("passport")) || (mail.ToLower().Contains("grubclub")))).ToList();

                if (lstmailid != null)
                {
                    SmtpSection smtpSection = (SmtpSection)(ConfigurationManager.GetSection("mailSettings/smtp_1"));

                    SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                    smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);

                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.From = new MailAddress(smtpSection.Network.UserName);
                    msg.To.Add(toAddress);
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    smtpClient.Send(msg);
                    smtpClient.Dispose();
                }

                if (lsthotmailid != null)
                {
                    SmtpSection smtpSection = (SmtpSection)(ConfigurationManager.GetSection("mailSettings/smtp_2"));

                    SmtpClient smtpClient = new SmtpClient(smtpSection.Network.Host, smtpSection.Network.Port);
                    smtpClient.Credentials = new NetworkCredential(smtpSection.Network.UserName, smtpSection.Network.Password);
                    smtpClient.EnableSsl = true;

                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.To.Add(toAddress); //Always send to self
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    smtpClient.Send(msg);
                    smtpClient.Dispose();
                }
                return true;
            }
            catch (Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
                return false;
            }
        }
        
        #endregion
                
        public EmailTemplate GetTemplate(int templateId)
        {
            EmailTemplate _emailTemplate = new EmailTemplate();
            try { 
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("GetEmailTemplate", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@TemplateId", templateId));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);

            if (ds != null)
            {
                _emailTemplate.Body = ds.Tables[0].Rows[0][0].ToString();
                _emailTemplate.Subject = ds.Tables[0].Rows[0][1].ToString();
                _emailTemplate.IsHtml = bool.Parse(ds.Tables[0].Rows[0][2].ToString());
            }
            }
            catch (Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
            }
            return _emailTemplate;
        }
        public bool AddReviewEmailNotificationLog(int eventId, string email, bool isFriend, Guid? userId = null)
        {
            emailSentStatus = false;
            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = Configuration.ConnectionString;
                SqlCommand command = new SqlCommand("AddReviewEmailNotificationLog", con);
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@EventId", eventId));
                command.Parameters.Add(new SqlParameter("@Email", email));
                command.Parameters.Add(new SqlParameter("@IsFriend", isFriend));
                if(userId != null)
                    command.Parameters.Add(new SqlParameter("@UserId", userId));
                
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds);

                if (ds != null)
                {
                    emailSentStatus = bool.Parse(ds.Tables[0].Rows[0][0].ToString() == "1" ? "true" : "false");
                }
            }
            catch(Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
            }
            return emailSentStatus;
        }
        public bool UpdateReviewEmailNotificationLog(int rowId)
        {
            emailSentStatus = false;
            try{
            SqlConnection con = new SqlConnection();
            con.ConnectionString = Configuration.ConnectionString;
            SqlCommand command = new SqlCommand("UpdateReviewEmailNotificationLog", con);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.Add(new SqlParameter("@Id", rowId));
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(command);
            da.Fill(ds);

            if (ds != null)
            {
                emailSentStatus = bool.Parse(ds.Tables[0].Rows[0][0].ToString() == "1" ? "true" : "false");
            }
            }
            catch(Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
            }
            return emailSentStatus;
        }
    }
}
