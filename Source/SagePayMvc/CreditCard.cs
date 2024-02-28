using System;
using System.Linq.Expressions;
using System.Text;
using System.Web;

namespace SagePayMvc
{
    public class CreditCard
    {
        public string CardHolder { get; set; }
        public string CardNumber { get; set; }
        public string StartDate { get; set; }
        public string ExpiryDate { get; set; }
        public string IssueNumber { get; set; }
        public string CV2 { get; set; }
        public string CardType { get; set; }

       
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(BuildPropertyString(x => x.CardHolder, CardHolder));
            builder.Append(BuildPropertyString(x => x.CardNumber, CardNumber));
            builder.Append(BuildPropertyString(x => x.StartDate, StartDate));
            builder.Append(BuildPropertyString(x => x.ExpiryDate,ExpiryDate));
            builder.Append(BuildPropertyString(x => x.IssueNumber, IssueNumber));
            builder.Append(BuildPropertyString(x => x.CV2, CV2));
            builder.Append(BuildPropertyString(x => x.CardType, CardType));
            return builder.ToString();
        }

        string BuildPropertyString(Expression<Func<CreditCard, object>> expression, string value, bool optional)
        {
            if (optional && string.IsNullOrEmpty(value)) return null;

            string name = PropertyToName(expression);
            return string.Format("&{1}={2}", name, HttpUtility.UrlEncode(value));
        }


        string BuildPropertyString(Expression<Func<CreditCard, object>> expression, string value)
        {
            return BuildPropertyString(expression, value, false);
        }

        static string PropertyToName(Expression<Func<CreditCard, object>> expression)
        {
            return (expression.Body as MemberExpression).Member.Name;
        }
    }
}
