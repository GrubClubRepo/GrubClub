using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public enum VType
    {
        Referral=1,
        GiftVoucher=2
    }

   
  public  class UserVoucherTypeDetail
    {
      #region Database Properties

      public virtual int Id
      {
          get;
          set;
      }


      public virtual Guid UserId
      {
          get;
          set;
      }

      public virtual string FriendEmailId
      {
          get;
          set; 
      }


      public virtual DateTime CreatedDate
      {
          get;
          set;
      }


      public virtual int VType
      {
          get;
          set;
      }

      public virtual int VoucherType
      {
          get;
          set;
      }

      public virtual decimal Value
      {
          get;
          set;
      }

      public virtual int VoucherId
      {
          get;
          set;
      }

      public virtual Guid? BasketId
      {
          get;
          set;
      }

      public virtual string Name
      {
          get;
          set;
      }

    }
# endregion
}
