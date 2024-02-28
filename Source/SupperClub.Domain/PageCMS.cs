using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
  public class PageCMS
    {

      public virtual int Id
      {
          get;
          set;
      }
      public virtual string Page
      {
          get;
          set;
      }

      public virtual string Section
      {
          get;
          set;
      }

        public virtual string TextLine1
        {
            get;
            set;
        }


        public virtual string TextLine2
        {
            get;
            set;
        }

        public virtual string Link
        {
            get;
            set;
        }

        public virtual string ImagePath
        {
            get;
            set;
        }

    }
}
