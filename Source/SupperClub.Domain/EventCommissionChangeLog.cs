using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
   public class EventCommissionChangeLog
   {
       #region Primitive Properties

       public virtual int Id
       {
           get;
           set;
       }

       public virtual System.DateTime Date
       {
           get;
           set;
       }


       public virtual decimal OldCommission
       {
           get;
           set;
       }

       public virtual decimal NewCommission
       {
           get;
           set;
       }

       public virtual decimal OldCost
       {
           get;
           set;
       }
       public virtual decimal NewCost
       {
           get;
           set;
       }
       public virtual int EventId
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
