//------------------------------------------------------------------------------
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

namespace SupperClub.Domain
{
    public class MessageTemplate
    {
        #region Primitive Properties
    
        public virtual int Id
        {
            get;
            set;
        }       
    
        public virtual string MessageTemplateType
        {
            get;
            set;
        }

        public virtual string MessageBody
        {
            get;
            set;
        }

        public virtual string PushwooshMessageBody
        {
            get;
            set;
        }
        public virtual string AlertText
        {
            get;
            set;
        }

        #endregion
    }
}
