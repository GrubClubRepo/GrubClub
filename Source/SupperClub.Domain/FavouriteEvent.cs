﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupperClub.Domain
{
    public class FavouriteEvent
    {
        #region Primitive Properties

        public virtual int Id
        {
            get;
            set;
        }

        public virtual int EventId
        {
            get;
            set;
        }

        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }
        #endregion
      
    }
}
