// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizationServerProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the AuthorizationServerProvider type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;

    using Microsoft.Owin.Security.OAuth;

    /// <summary>
    /// Authorization provider
    /// </summary>
    public class AuthorizationServerProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>
        /// The key to store current client in the context
        /// </summary>
        public const string OwinContextClientKey = "client";

        /// <summary>
        /// The key to store current client in the context
        /// </summary>
        public const string OwinContextUserSessionKey = "userSession";

        /// <summary>
        /// The client providers
        /// </summary>
        private readonly ImmutableList<IClientProvider> clientProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationServerProvider"/> class.
        /// </summary>
        /// <param name="clientProviders">
        /// The client providers.
        /// </param>
        public AuthorizationServerProvider(IEnumerable<IClientProvider> clientProviders)
        {
            this.clientProviders = clientProviders.OrderByDescending(p => p.Priority).ToImmutableList();
        }

        /// <summary>
        /// Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
        /// present on the request. If the web application accepts Basic authentication credentials,
        /// context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web
        /// application accepts "client_id" and "client_secret" as form encoded POST parameters,
        /// context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
        /// If context.Validated is not called the request will not proceed further.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;
            context.TryGetFormCredentials(out clientId, out clientSecret);

            IClient client = null;
            foreach (var clientProvider in this.clientProviders)
            {
                client = await clientProvider.GetClientAsync(clientId, clientSecret);
                if (client != null)
                {
                    break;
                }
            }

            if (client != null)
            {
                context.Validated(client.ClientId);
                context.OwinContext.Set(OwinContextClientKey, client);
            }
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "password". This occurs when the user has provided name and password
        /// credentials directly into the client application's user interface, and the client application is using those to acquire an "access_token" and
        /// optional "refresh_token". If the web application supports the
        /// resource owner credentials grant type it must validate the context.Username and context.Password as appropriate. To issue an
        /// access token the context.Validated must be called with a new ticket containing the claims about the resource owner which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.3.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var client = context.OwinContext.Get<IClient>(OwinContextClientKey);
            if (client == null)
            {
                return;
            }

            var session = await client.AuthenticateUserAsync(context.UserName, context.Password);
            if (session == null)
            {
                return;
            }

            context.OwinContext.Set(OwinContextUserSessionKey, session);
            var identity = new ClaimsIdentity(session.User.UserId);
            context.Validated(identity);
        }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application may implement this call in order to do any final
        /// modification of the claims being used to issue access or refresh tokens. This call may also be used in order to add additional
        /// response parameters to the Token endpoint's json response body.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            var userSession = context.OwinContext.Get<UserSession>(OwinContextUserSessionKey);
            context.Properties.ExpiresUtc = userSession.Expiring;
            return base.TokenEndpoint(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of any other value. If the application supports custom grant types
        /// it is entirely responsible for determining if the request should result in an access_token. If context.Validated is called with ticket
        /// information the response body is produced in the same way as the other standard grant types. If additional response parameters must be
        /// included they may be added in the final TokenEndpoint call.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.5
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            // todo: implement custom extensions
            return base.GrantCustomExtension(context);
        }
    }
}
