using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Collections;
using System.Web;
using System.Web.Security;

namespace SupperClub.Domain
{
    public class TempUserIdLog
    {        
        #region Properties        
        public int Id { get; set; }
        public Guid UserId {get; set;}
        public DateTime DateCreated { get; set; }        
        #endregion
        
    }
}
