using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChefNewEventNotificationService.BLL
{
    public class EmailTemplate
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsHtml { get; set; }    
    }    

}
