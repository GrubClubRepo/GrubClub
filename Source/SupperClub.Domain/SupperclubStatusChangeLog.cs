using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class SupperclubStatusChangeLog
    {

        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        public virtual System.DateTime CreatedDate
        {
            get;
            set;
        }


        public virtual int OldStatus
        {
            get;
            set;
        }

        public virtual int Status
        {
            get;
            set;
        }

        public virtual int SupperClubId
        {
            get;
            set;
        }

      
        public virtual Guid UserId
        {
            get;
            set;
        }

        #endregion
    }
}
