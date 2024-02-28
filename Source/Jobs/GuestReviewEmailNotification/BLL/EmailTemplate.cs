using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GuestReviewEmailNotification.BLL
{
    public class EmailTemplate
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsHtml { get; set; }    
    }    
}
