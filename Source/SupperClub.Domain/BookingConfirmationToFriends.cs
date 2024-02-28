using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
  public  class BookingConfirmationToFriends
    {
       #region Database Properties

      public virtual int Id
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

      public virtual string FriendsMailIds
      {
          get;
          set; 
      }


      public virtual DateTime CreatedDate
      {
          get;
          set;
      }

      public virtual string Message
      {
          get;
          set;
      }

    }
# endregion
    
}
