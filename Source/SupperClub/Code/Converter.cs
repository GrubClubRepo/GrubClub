using System;
using System.Globalization;
using System.Collections.Generic;

namespace SupperClub.Code
{
    /// <summary>
    /// Contains methods converting decimals and dates.
    /// </summary>
    public class Converter 
    {
        /// <summary>
        ///  british Culture Info.
        /// </summary>
        private static CultureInfo britishCultureInfo = new CultureInfo("en-GB");

        /// <summary>
        /// Converts decimal to string with dot.
        /// </summary>
        /// <param name="number">Number as decimal.</param>
        /// <returns>Number as string.</returns>
        public static string DecimalToString(decimal? number)
        {
            if (number != null)
            {
                decimal decnum = (decimal)number;
                return decnum.ToString(System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
            }
            return "";

        }

        /// <summary>
        /// Converts string to decimal.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>Number as decimal.</returns>
        public static decimal? StringToDecimal(string number)
        {
            decimal res;
            if (decimal.TryParse(number.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture.NumberFormat, out res))
            {
                return res;
            }
            else
                return null;
        }

        /// <summary>
        /// Convert strings to date time (dd/mm/yyyy).
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Date as DateTime. </returns>
        public static DateTime StringToDateTime(string date)
        {
            return DateTime.Parse(date, britishCultureInfo);
        }

        /// <summary>
        /// Converts date to string (dd/mm/yyyy).
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns>Date as string.</returns>
        public static string DateTimeToString(DateTime date)
        {
            return date.ToString("dd/MM/yyyy", britishCultureInfo);
        }

        /// <summary>
        /// Replaces keywords in the template with given values.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="toReplace">The strings to replace. Key is value to replace, Value is replacement value</param>
        /// <returns>Template with changed values.</returns>   
        public static string ReplaceTemplate(string template, Dictionary<string, string> toReplace)
        {
            if (toReplace != null)
            {
                foreach (KeyValuePair<string, string> kvp in toReplace)
                {
                    template = template.Replace("[" + kvp.Key + "]", kvp.Value);
                }
            }
            return template;
        }
    }
}