using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;

namespace SupperClub.Domain
{
    public class NewlyCreatedEvent
    {
        public virtual int EventId
        {
            get;
            set;
        }
                
        public virtual DateTime DateAdded
        {
            get;
            set;
        }
        public virtual bool IsProcessed
        {
            get;
            set;
        }  
    }
}
