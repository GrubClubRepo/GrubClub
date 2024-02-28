using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class PressRelease
    {
        public virtual int Id
        {
            get;
            set;
        }

        public virtual string Content
        {
            get;
            set;
        }
        public virtual string Url
        {
            get;
            set;
        }       
        public virtual string LogoImageUrl
        {
            get;
            set;
        }
    }
}
