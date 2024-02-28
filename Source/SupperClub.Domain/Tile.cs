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
    public class Tile
    {
        
        #region Constructors
        public Tile()
        {
        }

        public Tile(int pagetype, int tileNumber)
        {
            this.PageType = pagetype;
            this.TileNumber = tileNumber;
        }
        #endregion

        #region Properties
        
        public int Id { get; set; }
        public int PageType {get; set;}
        public int TileNumber { get; set; }
        //public virtual TileTag TileTag { get; set; }

        #endregion
        
    }
}
