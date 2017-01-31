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

    /// <summary>
    /// The api controller extension to acquire authenticated user data
    /// </summary>
    public static class AuthorizedController
    {
        /// <summary>
        /// Gets authenticated user session or null
        /// </summary>
        /// <param name="controller">The api controller</param>
        /// <returns>The user session</returns>
        public static UserSession GetSession(this ApiController controller)
        {
            var context = controller.Request.GetOwinContext();
            return context?.Get<UserSession>("UserSession");
        }
    }
}
