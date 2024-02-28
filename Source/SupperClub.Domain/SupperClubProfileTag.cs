using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
  public  class SupperClubProfileTag
    {

        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        public virtual int SupperClubId
        {
            get { return _supperClubId; }
            set
            {
                if (_supperClubId != value)
                {
                    if (SupperClub != null && SupperClub.Id != value)
                    {
                        SupperClub = null;
                    }
                    _supperClubId = value;
                }
            }
        }
        private int _supperClubId;


        public virtual DateTime DateCreated
        {
            get;
            set;
            
        }
        

        public virtual int ProfileTagId
        {
            get { return _profileTagId; }
            set
            {
                if (_profileTagId != value)
                {
                    if (ProfileTag != null && ProfileTag.Id != value)
                    {
                        ProfileTag = null;
                    }
                    _profileTagId = value;
                }
            }
        }
        private int _profileTagId;

        #endregion

        #region Navigation Properties

        public virtual ProfileTag ProfileTag
        {
            get { return _profileTag; }
            set
            {
                if (!ReferenceEquals(_profileTag, value))
                {
                    var previousValue = _profileTag;
                    _profileTag = value;
                    FixupProfileTag(previousValue);
                }
            }
        }
        private ProfileTag _profileTag;

        public virtual SupperClub SupperClub
        {
            get { return _supperClub; }
            set
            {
                if (!ReferenceEquals(_supperClub, value))
                {
                    var previousValue = _supperClub;
                    _supperClub = value;

                    FixupSupperClub(previousValue);
                }
            }
        }
        private SupperClub _supperClub;

        #endregion

        #region Association Fixup

        private void FixupProfileTag(ProfileTag previousValue)
        {
            if (previousValue != null && previousValue.SupperClubTags.Contains(this))
            {
                previousValue.SupperClubTags.Remove(this);
            }

            if (ProfileTag != null)
            {
                if (!ProfileTag.SupperClubTags.Contains(this))
                {
                    ProfileTag.SupperClubTags.Add(this);
                }
                if (ProfileTagId != ProfileTag.Id)
                {
                    ProfileTagId = ProfileTag.Id;
                }
            }
        }

        private void FixupSupperClub(SupperClub previousValue)
        {
            if (previousValue != null && previousValue.SupperClubProfileTags.Contains(this))
            {
                previousValue.SupperClubProfileTags.Remove(this);
            }

            if (SupperClub != null)
            {
                if (!SupperClub.SupperClubProfileTags.Contains(this))
                {
                    SupperClub.SupperClubProfileTags.Add(this);
                }
                if (SupperClubId != SupperClub.Id)
                {
                    SupperClubId = SupperClub.Id;
                }
            }
        }

        #endregion
    }
}
