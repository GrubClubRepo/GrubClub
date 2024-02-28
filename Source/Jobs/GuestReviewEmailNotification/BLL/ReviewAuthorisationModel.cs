using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Net;

namespace GuestReviewEmailNotification.BLL
{
    public class SimplerAES
    {
        // Static keys for simplicity. This is only used to decode a Review auth token back to a specific eventId and userId
        private static byte[] key = { 8, 217, 19, 11, 5, 26, 85, 45, 114, 4, 27, 162, 37, 112, 222, 34, 241, 24, 175, 144, 16, 53, 196, 29, 24, 26, 17, 218, 131, 236, 53, 209 };
        private static byte[] vector = { 5, 64, 191, 111, 23, 3, 113, 10, 231, 121, 221, 112, 79, 32, 18, 156 };
        private ICryptoTransform encryptor, decryptor;
        private UTF8Encoding encoder;

        public SimplerAES()
        {
            RijndaelManaged rm = new RijndaelManaged();
            encryptor = rm.CreateEncryptor(key, vector);
            decryptor = rm.CreateDecryptor(key, vector);
            encoder = new UTF8Encoding();
        }

        public string Encrypt(string unencrypted)
        {
            return Convert.ToBase64String(Encrypt(encoder.GetBytes(unencrypted)));
        }

        public string Decrypt(string encrypted)
        {
            return encoder.GetString(Decrypt(Convert.FromBase64String(encrypted)));
        }

        public string EncryptToUrl(string unencrypted)
        {
            return WebUtility.UrlEncode(Encrypt(unencrypted));
        }

        public string DecryptFromUrl(string encrypted)
        {
            return Decrypt(WebUtility.UrlDecode(encrypted));
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, encryptor);
        }

        public byte[] Decrypt(byte[] buffer)
        {
            return Transform(buffer, decryptor);
        }

        protected byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            MemoryStream stream = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(stream, transform, CryptoStreamMode.Write))
            {
                cs.Write(buffer, 0, buffer.Length);
            }
            return stream.ToArray();
        }
    }
    public class ReviewAuthorisationModel
    {
        // These keys have been padded to be the same length as a userId Guid to simulate a Guid
        private static string AllowGuestKey = "GuestsOk-c2cf-4217-82c0-753e453adeb8";
        private static string RequiresLogonKey = "RequiresLogon-4217-82c0-753e453adeb8";

        public int EventId { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }

        public string Encrypt()
        {
            string toEncrypt = string.Format("eventId={0};userId={1};email={2}", this.EventId, this.UserId, this.Email);
            SimplerAES sa = new SimplerAES();
            return sa.EncryptToUrl(toEncrypt);
        }

        public string Encrypt(int eventId, bool allowGuests, string email)
        {
            this.EventId = eventId;
            this.UserId = allowGuests ? AllowGuestKey : RequiresLogonKey;
            this.Email = email;
            return Encrypt();
        }

        public string Encrypt(int eventId, string userId)
        {
            this.EventId = eventId;
            this.UserId = userId;
            this.Email = string.Empty;
            return Encrypt();
        }

        public bool Decrypt(string toDecrypt)
        {
            try
            {
                SimplerAES sa = new SimplerAES();
                string reviewerInfo = sa.Decrypt(toDecrypt); // URL is already converted by MVC don't need to use sa.DecryptUrl
                string[] keys = reviewerInfo.Split(';');
                string[] eventKey = keys[0].Split('=');
                string[] userKey = keys[1].Split('=');
                string[] emailKey = keys[2].Split('=');

                this.EventId = int.Parse(eventKey[1]);
                this.UserId = userKey[1];
                this.Email = emailKey[1];
                return true;
            }
            catch
            {
                // Unable to decrypt the key
                return false;
            }
        }

        public bool AllowGuests
        {
            get { return (this.UserId == AllowGuestKey); }
        }

        public bool AllUsers
        {
            get { return (this.UserId == AllowGuestKey || this.UserId == RequiresLogonKey); }
        }

    }
}