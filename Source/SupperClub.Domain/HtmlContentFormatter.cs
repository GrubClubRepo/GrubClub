using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Text.RegularExpressions;

namespace SupperClub.Domain
{
    public class HtmlContentFormatter
    {
       
        private static int findStringNthIndex(string target, string value, int nth)
        {
            int i = 1;
            int index = 0;

            while (i <= nth && (index = target.IndexOf(value, index + 1)) != -1)
            {
                if (i == nth)
                    return index;

                i++;
            }

            return -1;
        }

        
        public static string GetSubString(string inputString, int length)
        {

            string _outputString = inputString;
            if (!string.IsNullOrEmpty(_outputString) && _outputString.Length > length)
            {
                bool linkFound = false;
                if (_outputString.Substring(0, length).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "<a").Count == System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "</a>").Count))
                {
                    _outputString = _outputString.Substring(0, length) + "...";
                }
                else
                {
                    int indexer = System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "<a").Count;
                    int requiredStringLength = findStringNthIndex(_outputString, "</a>", indexer);
                    _outputString = _outputString.Substring(0, requiredStringLength + 4) + "...";
                    linkFound = true;
                }
                if (_outputString.Substring(0, length).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "</strong>").Count))
                {
                    if (!linkFound)
                        _outputString = _outputString.Substring(0, length) + "...";
                }
                else
                {
                    if (!linkFound)
                        _outputString = _outputString.Substring(0, length) + "</strong>...";
                    else
                        _outputString = _outputString + "</strong>...";
                }
                if (_outputString.Substring(0, length).IndexOf("<em>") == -1 || (System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "<em>").Count == System.Text.RegularExpressions.Regex.Matches(_outputString.Substring(0, length), "</em>").Count))
                {
                    if (!linkFound)
                        _outputString = _outputString.Substring(0, length) + "...";
                }
                else
                {
                    if (!linkFound)
                        _outputString = _outputString.Substring(0, length) + "</em>...";
                    else
                        _outputString = _outputString + "</em>...";
                }
            }
            return _outputString;
        }
    }
}
