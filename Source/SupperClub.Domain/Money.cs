using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace DataAnnotationsExtensions
{
    public class MoneyAttribute : ValidationAttribute
    {

        public override bool IsValid(object value)
        {
            var number = Convert.ToString(value).Replace("£", "");

            if (String.IsNullOrEmpty(number))
                return true;
            decimal outnumber;
            return Decimal.TryParse(number, out outnumber);

        }

        public override string FormatErrorMessage(string name)
        {
            return "The " + name + " is not a valid currency.";
        }


        
    }
}