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
    using System.Net.Http;
    using System.Web.Http;

    using ClusterKit.Security.Client;

    using Microsoft.Owin;

    /// <summary>
    /// The api controller extension to acquire authenticated user data
    /// </summary>
    public static class AuthorizedController
    {
        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="context">The owin context</param>
        /// <returns>The user session</returns>
        public static UserSession GetSession(this IOwinContext context)
        {
            return context?.Get<UserSession>("UserSession");
        }

        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        public static UserSession GetSession(this ApiController controller)
        {
            var context = controller.Request.GetOwinContext();
            return context?.GetSession();
        }
    }
}
