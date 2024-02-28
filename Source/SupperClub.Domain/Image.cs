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
    public enum ImageType
    {
        Event = 1,
        SupperClub = 2,
        Temp = 3
    }
    public class Image
    {
        
        #region Constructors
        public Image()
        {
        }

        public Image(string url)
        {
            this.Url = url;
        }
        #endregion

        #region Properties
        
        public int Id { get; set; }
        public string Url {get; set;}
        public bool Active { get; set; }
        public virtual List<TileTag> TileTags { get; set; }

        #endregion

        
    }
}
