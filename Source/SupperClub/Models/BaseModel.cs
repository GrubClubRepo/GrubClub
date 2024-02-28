using System;
using System.Web;
using SupperClub.Models;
using SupperClub.Data;
using SupperClub.Domain.Repository;
using System.Web.Mvc;

namespace SupperClub.Models
{
    /// <summary>
    /// Represents Base model.
    /// </summary>
    public class BaseModel
    {
        /// <summary>
        /// Gets Request Data Context.
        /// </summary>
        /// <value>Request Data Context.</value>
        public static ISupperClubRepository DB
        {
            get
            {
                if (HttpContext.Current.Items["DomainDataContext"] == null)
                {
                    ISupperClubRepository db = DependencyResolver.Current.GetService<ISupperClubRepository>();
                    HttpContext.Current.Items["DomainDataContext"] = db;
                }
                return (ISupperClubRepository)HttpContext.Current.Items["DomainDataContext"];
            }
        }

        /// <summary>
        /// Gets object from session.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <param name="sessionKey">The session key.</param>
        /// <param name="func">The function which gets this object.</param>
        /// <returns>Object from session.</returns>
        public static TSource GetFromSession<TSource>(string sessionKey, Func<TSource> func)
        {
            if (HttpContext.Current.Session[sessionKey] != null)
                return (TSource)HttpContext.Current.Session[sessionKey];
            else
            {
                TSource source = func.Invoke();
                HttpContext.Current.Session[sessionKey] = source;
                return source;
            }
        }
    }
}