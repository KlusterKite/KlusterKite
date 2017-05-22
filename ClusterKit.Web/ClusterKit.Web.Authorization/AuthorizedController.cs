// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizedController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api controller extension to acquire authenticated user data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authorization
{
    using System.Linq;
    using System.Net.Http;
    using System.Reflection;
    using System.Web.Http;

    using ClusterKit.Security.Attributes;

    using JetBrains.Annotations;

    using Microsoft.Owin;

    /// <summary>
    /// The api controller extension to acquire authenticated user data
    /// </summary>
    public static class AuthorizedController
    {
        /// <summary>
        /// Gets security request description
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <returns>The security request description</returns>
        [UsedImplicitly]
        public static RequestContext GetRequestDescription(this IOwinContext context)
        {
            return new RequestContext
                       {
                           Authentication = context.GetSession(),
                           RemoteAddress = context.Request.RemoteIpAddress,
                           RequestedLocalUrl = context.Request.Uri.ToString(),
                           Headers =
                               context.Request.Headers.Where(p => p.Key?.ToLower().Trim() != "authorization")
                                   .ToDictionary(p => p.Key, p => string.Join("; ", p.Value))
                       };
        }

        /// <summary>
        /// Gets security request description
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The security request description</returns>
        [UsedImplicitly]
        public static RequestContext GetRequestDescription(this ApiController controller)
        {
            var context = controller.Request.GetOwinContext();
            return context.GetRequestDescription();
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <returns>The user session</returns>
        public static AccessTicket GetSession(this IOwinContext context)
        {
            return context.Get<AccessTicket>("AccessTicket");
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        public static AccessTicket GetSession(this ApiController controller)
        {
            var context = controller.Request.GetOwinContext();
            return context.GetSession();
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <returns>The user session</returns>
        [UsedImplicitly]
        public static string GetAuthenticationToken(this IOwinContext context)
        {
            return context.Get<string>("Token");
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        [UsedImplicitly]
        public static string GetAuthenticationToken(this ApiController controller)
        {
            var context = controller.Request.GetOwinContext();
            return context.GetAuthenticationToken();
        }
    }
}
