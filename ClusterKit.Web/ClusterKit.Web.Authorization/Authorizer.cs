// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Authorizer.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Checks the access token for validness and extracts the user session data
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Web.Authorization
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Microsoft.Owin;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Infrastructure;

    /// <summary>
    /// Checks the access token for validness and extracts the user session data
    /// </summary>
    [UsedImplicitly]
    public class Authorizer : AuthenticationMiddleware<Authorizer.AuthorizerOptions>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Authorizer"/> class.
        /// </summary>
        /// <param name="next">
        /// The next.
        /// </param>
        /// <param name="options">
        /// The options.
        /// </param>
        public Authorizer(OwinMiddleware next, AuthorizerOptions options)
            : base(next, options)
        {
        }

        /// <inheritdoc />
        protected override AuthenticationHandler<AuthorizerOptions> CreateHandler()
        {
            return new AuthorizerHandler();
        }

        /// <summary>
        /// The list of authentication options
        /// </summary>
        public class AuthorizerOptions : AuthenticationOptions
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AuthorizerOptions"/> class.
            /// </summary>
            /// <param name="authenticationType">
            /// The authentication type.
            /// </param>
            /// <param name="tokenManager">
            /// The token manager.
            /// </param>
            public AuthorizerOptions(string authenticationType, ITokenManager tokenManager)
                : base(authenticationType)
            {
                this.TokenManager = tokenManager;
            }

            /// <summary>
            /// Gets the token manager
            /// </summary>
            public ITokenManager TokenManager { get; }
        }

        /// <summary>
        /// The authentication handler
        /// </summary>
        private class AuthorizerHandler : AuthenticationHandler<AuthorizerOptions>
        {
            /// <inheritdoc />
            protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
            {
                var authorizationHeader = this.Context.Request.Headers["Authorization"];
                if (string.IsNullOrWhiteSpace(authorizationHeader))
                {
                    return null;
                }

                var authParts = authorizationHeader.Split(' ');
                if (authParts.Length != 2 || authParts[0].ToLower() != "bearer"
                    || string.IsNullOrWhiteSpace(authParts[1]))
                {
                    return null;
                }

                var token = authParts[1];
                var session = await this.Options.TokenManager.ValidateAccessToken(token);
                if (session == null)
                {
                    this.Context.Set("InvalidToken", true);
                    return null;
                }

                this.Context.Set("AccessTicket", session);
                this.Context.Set("Token", token);

                return new AuthenticationTicket(
                    new ClaimsIdentity(session.User?.UserId ?? session.ClientId),
                    new AuthenticationProperties { IssuedUtc = session.Created, ExpiresUtc = session.Expiring });
            }
        }
    }
}