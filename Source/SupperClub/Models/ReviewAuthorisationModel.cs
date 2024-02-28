using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SupperClub.Code;

namespace SupperClub.Models
{
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

        public string Encrypt(int eventId, bool allowGuests, string email = null)
        {
            this.EventId = eventId;
            this.UserId = allowGuests ? AllowGuestKey : RequiresLogonKey;
            this.Email = email == null ? string.Empty:email;
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

                string[] emailKey = null;
                if(keys.Length > 2)
                    emailKey = keys[2].Split('=');

                this.EventId = int.Parse(eventKey[1]);
                this.UserId = userKey[1];
                if (keys.Length > 2)
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