using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
   
   public class PushNotificationLog
    {

        #region Database Properties

        public int Id
        {
            get;
            set;
        }
        public DateTime LogDate
        {
            get;
            set;
        }
        public int PushNotificationTypeId
        {
            get;
            set;
        }
        public string DeviceTokens
        {
            get;
            set;
        }
        public string MessageText
        {
            get;
            set;
        }
        public bool Sent
        {
            get;
            set;
        }
        #endregion

    }
}
