using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public enum SearchSourcePageType
    {
        Search = 1,
        Home = 2
    }
   public class SearchLogDetail
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

        public virtual DateTime CreatedDate
        {
            get;
            set;
        }


        public virtual string Keyword
        {
            get;
            set;
        }

        public virtual string Price
        {
            get;
            set;
        }


        public virtual int? Distance
        {
            get;
            set;
        }

        public virtual string Postcode
        {
            get;
            set;
        }


        public virtual DateTime? StartDate
        {
            get;
            set;
        }

        public virtual DateTime? EndDate
        {
            get;
            set;
        }


        public virtual string  Cusine
        {
            get;
            set;
        }

        public virtual string Diet
        {
            get;
            set;
        }


        public virtual bool? Byob
        {
            get;
            set;
        }

        public virtual bool? Charity
        {
            get;
            set;
        }
        
       public virtual int SourcePageTypeId
        {
            get;
            set;
        }
        #endregion

    }
}
