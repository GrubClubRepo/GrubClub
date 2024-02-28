using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mail;
using System.Net;

namespace PopularGrubClubs
{
    class Program
    {
        static void Main(string[] args)
        {
            

             SqlConnection con = new SqlConnection();
             con.ConnectionString = "data source=185.43.77.95\\SQLEXPRESS,51356;Network Library=DBMSSOCN;Initial Catalog=SupperClub;User Id=supperclub_user;Password=2L!3vC5?db";//ConfigurationManager.ConnectionStrings["SupperClubConnectionString"].ToString();
            con.Open();
            try
            {
               
                SqlCommand command = new SqlCommand("SavePopularEvents", con);
                command.CommandType = CommandType.StoredProcedure;

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds);

                if (ds != null)
                {
                    string commandText = "Insert into SavePopularEventStatus(Status,Message) Values(" + ds.Tables[0].Rows[0][0].ToString() + ",'" + ds.Tables[0].Rows[0][1].ToString() + "')";
                    SqlCommand command2 = new SqlCommand(commandText, con);
                    command2.ExecuteNonQuery();

                }
            }
            catch (Exception ex)
            {
                string commandText = "Insert into SavePopularEventStatus(Status,Message) Values(0,'" + ex.Message + "')";
                SqlCommand command2 = new SqlCommand(commandText, con);
                command2.ExecuteNonQuery();

                SmtpClient client = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("admin@grubclub.com", "supperclubliv")
                };

                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("admin@grubclub.com");
                msg.Subject = "Save Popular Event Job Failed";
                msg.Body = "Save Popular Event Job Failed due to the exception:" + ex.Message;
                msg.BodyEncoding = System.Text.Encoding.ASCII;

                msg.To.Add("supraja@grubclub.com");
                // client.EnableSsl = true;
                client.Send(msg);
                client.Dispose();
                

              

            }
            finally
            {
                con.Close();
                con.Dispose();
            }



             
        }
    }
}
