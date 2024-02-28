using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class TileTag
    {
        public TileTag()
        {
        }

        public TileTag(int tileId, int tagId, int imageId, bool active)
        {
            this.TileId = tileId;
            this.TagId = tagId;
            this.ImageId = imageId;
            this.Active = active;
        }

        public DateTime LastUpdatedDate { get; set; }
        public bool Active { get; set; }

        public int TileId { get; set; }
        //[ForeignKey("TileId")]       
        //public virtual Tile Tile { get; set; }

        public int TagId { get; set; }
        //[ForeignKey("TagId")]
        //public virtual Tag Tag { get; set; }

        public int ImageId { get; set; }
        //[ForeignKey("ImageId")]
        //public virtual Image Image { get; set; }

        #region Navigation Properties
        public Tile Tile
        {
            get { return _tile; }
            set
            {
                if (!ReferenceEquals(_tile, value))
                {
                    var previousValue = _tile;
                    _tile = value;
                }
            }
        }
        private Tile _tile;
        
        public Tag Tag
        {
            get { return _tag; }
            set
            {
                if (!ReferenceEquals(_tag, value))
                {
                    var previousValue = _tag;
                    _tag = value;
                    //FixupSupperClub(previousValue);
                }
            }
        }
        private Tag _tag;

        public Image Image
        {
            get { return _image; }
            set
            {
                if (!ReferenceEquals(_image, value))
                {
                    var previousValue = _image;
                    _image = value;
                    //FixupSupperClub(previousValue);
                }
            }
        }
        private Image _image;
        #endregion
    }
}
