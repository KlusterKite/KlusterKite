// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationExtensionsController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Extensions to the standard OAuth2 methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System.Threading.Tasks;

    using ClusterKit.Security.Attributes;
    using ClusterKit.Web.Authorization;
    using ClusterKit.Web.Authorization.Attributes;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Extensions to the standard OAuth2 methods
    /// </summary>
    [Route("api/1.x/security/extensions")]
    public class AuthenticationExtensionsController : Controller
    {
        /// <summary>
        /// Current token manager
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationExtensionsController"/> class.
        /// </summary>
        /// <param name="tokenManager">
        /// The token manager.
        /// </param>
        public AuthenticationExtensionsController(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        /// <summary>
        /// Revokes the current access token
        /// </summary>
        /// <returns>The success of the operation</returns>
        [Route("revokeSelf")]
        [HttpGet]
        [RequireSession]
        public async Task<bool> RevokeSelf()
        {
            return await this.tokenManager.RevokeAccessToken(this.GetAuthenticationToken());
        }
    }
}
