using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class UrlRewrite
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual string RewriteUrl
        {
            get;
            set;
        }
       
        public virtual string ActualUrl
        {
            get;
            set;
        }
        public virtual bool Active
        {
            get;
            set;
        }
    }
}
