// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizedController.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The api controller extension to acquire authenticated user data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authorization
{
    using System.Linq;
    
    using KlusterKite.Security.Attributes;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// The api controller extension to acquire authenticated user data
    /// </summary>
    public static class AuthorizedController
    {
        /// <summary>
        /// Gets security request description
        /// </summary>
        /// <param name="context">The http context</param>
        /// <returns>The security request description</returns>
        [UsedImplicitly]
        public static RequestContext GetRequestDescription(this HttpContext context)
        {
            return new RequestContext
                       {
                           Authentication = context.GetSession(),
                           RemoteAddress = context.Connection.RemoteIpAddress.ToString(),
                           RequestedLocalUrl = context.Request.Path,
                           Headers =
                               context.Request.Headers.ToList().Where(p => p.Key?.ToLower().Trim() != "authorization")
                                   .ToDictionary(p => p.Key, p => string.Join("; ", p.Value.ToArray()))
                       };
        }

        /// <summary>
        /// Gets security request description
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The security request description</returns>
        [UsedImplicitly]
        public static RequestContext GetRequestDescription(this Controller controller)
        {
            return controller.HttpContext.GetRequestDescription();
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="context">The context</param>
        /// <returns>The user session</returns>
        public static AccessTicket GetSession(this HttpContext context)
        {
            object ticket;
            if (context.Items.TryGetValue("AccessTicket", out ticket))
            {
                return (AccessTicket)ticket;
            }

            return null;
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        public static AccessTicket GetSession(this Controller controller)
        {
            return controller.HttpContext.GetSession();
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="context">The http context</param>
        /// <returns>The user session</returns>
        [UsedImplicitly]
        public static string GetAuthenticationToken(this HttpContext context)
        {
            object token;
            if (context.Items.TryGetValue("Token", out token))
            {
                return (string)token;
            }

            return null;
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        [UsedImplicitly]
        public static string GetAuthenticationToken(this Controller controller)
        {
            return controller.HttpContext.GetAuthenticationToken();
        }
    }
}
