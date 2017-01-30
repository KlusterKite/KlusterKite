// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Authentication controller
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Auth
{
    using System.Net.Http;
    using System.Security.Claims;
    using System.Web.Http;

    using Microsoft.Owin.Security;

    /// <summary>
    /// Authentication controller
    /// </summary>
    [RoutePrefix("api/1.0/security")]
    public class AuthenticationController : ApiController
    {
        /// <summary>
        /// Authenticate user
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        [HttpPost]
        [Route("token")]
        public string Token()
        {
            return "ok";
        }
    }
}
