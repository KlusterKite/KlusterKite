// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefreshTokenProvider.cs" company="ClusterKit">
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

    using ClusterKit.Security.Attributes;

    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    /// <summary>
    /// Creates authentication tokens
    /// </summary>
    public class RefreshTokenProvider : AuthenticationTokenProvider
    {
        /// <summary>
        /// The token manager
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenProvider"/> class.
        /// </summary>
        /// <param name="tokenManager">
        /// The token manager.
        /// </param>
        public RefreshTokenProvider(ITokenManager tokenManager)
        {
            this.tokenManager = tokenManager;
        }

        /// <inheritdoc />
        public override async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var authenticationResult = context.OwinContext.Get<AuthenticationResult>(AuthorizationServerProvider.OwinContextAuthenticationResultKey);
            if (authenticationResult.RefreshTicket != null)
            {
                var token = await this.tokenManager.CreateRefreshToken(authenticationResult.RefreshTicket);
                context.SetToken(token);
            }
        }

        /// <inheritdoc />
        public override async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var tokenValue = context.Token;
            var identity = await this.tokenManager.ValidateRefreshToken(tokenValue);
            if (identity != null)
            {
                context.OwinContext.Set(AuthorizationServerProvider.OwinContextRefreshTicketKey, identity);
                context.SetTicket(
                    new AuthenticationTicket(
                        new ClaimsIdentity(identity.UserId ?? identity.ClientId),
                        new AuthenticationProperties { ExpiresUtc = identity.Expiring, IssuedUtc = identity.Created }));
            }
        }
    }
}
