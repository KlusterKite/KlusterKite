// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccessTokenProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates authentication tokens
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System.Security.Claims;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    /// <summary>
    /// Creates authentication tokens
    /// </summary>
    public class AccessTokenProvider : AuthenticationTokenProvider
    {
        /// <summary>
        /// The token manager
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessTokenProvider"/> class.
        /// </summary>
        /// <param name="tokenManager">
        /// The token manager.
        /// </param>
        public AccessTokenProvider(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        /// <inheritdoc />
        public override async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var authenticationResult = context.OwinContext.Get<AuthenticationResult>(AuthorizationServerProvider.OwinContextAuthenticationResultKey);
            var token = await this.tokenManager.CreateAccessToken(authenticationResult.AccessTicket);
            context.SetToken(token);
        }

        /// <inheritdoc />
        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var tokenValue = context.Token;
            var session = await this.tokenManager.ValidateAccessToken(tokenValue);
            if (session != null)
            {
                context.SetTicket(
                    new AuthenticationTicket(
                        new ClaimsIdentity(session.User?.UserId ?? session.ClientId),
                        new AuthenticationProperties { ExpiresUtc = session.Expiring, IssuedUtc = session.Created }));
            }
        }
    }
}
