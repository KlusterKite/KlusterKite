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
    using ClusterKit.Web.Authorization;

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
        public const string OwinContextAuthenticationResultKey = "authenticationData";

        /// <summary>
        /// The key to store current client in the context
        /// </summary>
        public const string OwinContextRefreshTicketKey = "refreshTicket";

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
            else
            {
                var message = string.IsNullOrWhiteSpace(clientSecret)
                                 ? "Failed authentication for client {ClientId} without secret"
                                 : "Failed authentication for client {ClientId} with secret";
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied, 
                    EnSeverity.Crucial, 
                    context.OwinContext.GetRequestDescription(),
                    message,
                    clientId);
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
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Unexpected operation. Grant password authentication without client data attempt.");
                return;
            }

            var result = await client.AuthenticateUserAsync(context.UserName, context.Password);
            if (result?.AccessTicket.User == null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Grant password failed for user {UserName} with client {ClientId}",
                    context.UserName,
                    client.ClientId);

                return;
            }

            context.OwinContext.Set(OwinContextAuthenticationResultKey, result);
            var identity = new ClaimsIdentity(result.AccessTicket.User.UserId);
            context.Validated(identity);

            SecurityLog.CreateRecord(
                SecurityLog.EnType.AuthenticationGranted,
                EnSeverity.Crucial,
                context.OwinContext.GetRequestDescription(),
                "Grant password was successfull for user {UserName} with client {ClientId}",
                context.UserName,
                client.ClientId);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "client_credentials". This occurs when a registered client
        /// application wishes to acquire an "access_token" to interact with protected resources on it's own behalf, rather than on behalf of an authenticated user.
        /// If the web application supports the client credentials it may assume the context.ClientId has been validated by the ValidateClientAuthentication call.
        /// To issue an access token the context.Validated must be called with a new ticket containing the claims about the client application which should be associated
        /// with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        /// The default behavior is to reject this grant type.
        /// See also http://tools.ietf.org/html/rfc6749#section-4.4.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override async Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            var client = context.OwinContext.Get<IClient>(OwinContextClientKey);
            if (client == null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Unexpected operation. Grant client_credentials authentication without client data attempt.");
                return;
            }

            var result = await client.AuthenticateSelf();
            if (result == null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Grant client_credentials denied for {ClientId}",
                    client.ClientId);

                return;
            }

            context.OwinContext.Set(OwinContextAuthenticationResultKey, result);
            var identity = new ClaimsIdentity(result.AccessTicket.ClientId);
            context.Validated(identity);
            SecurityLog.CreateRecord(
                SecurityLog.EnType.AuthenticationGranted,
                EnSeverity.Crucial,
                context.OwinContext.GetRequestDescription(),
                "Grant client_credentials was successfull for client {ClientId}",
                client.ClientId);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token". This occurs if your application has issued a "refresh_token"
        /// along with the "access_token", and the client is attempting to use the "refresh_token" to acquire a new "access_token", and possibly a new "refresh_token".
        /// To issue a refresh token the an Options.RefreshTokenProvider must be assigned to create the value which is returned. The claims and properties
        /// associated with the refresh token are present in the context.Ticket. The application must call context.Validated to instruct the
        /// Authorization Server middleware to issue an access token based on those claims and properties. The call to context.Validated may
        /// be given a different AuthenticationTicket or ClaimsIdentity in order to control which information flows from the refresh token to
        /// the access token. The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the refresh token to
        /// the access token unmodified.
        /// See also http://tools.ietf.org/html/rfc6749#section-6
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>Task to enable asynchronous execution</returns>
        public override async Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var refreshTicket = context.OwinContext.Get<RefreshTicket>(OwinContextRefreshTicketKey);
            var client = context.OwinContext.Get<IClient>(OwinContextClientKey);

            if (refreshTicket == null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Unexpected operation. Grant refresh_token authentication without validated refresh ticket.");

                return;
            }

            if (client == null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Unexpected operation. Grant refresh_token authentication without client data attempt.");
                return;
            }

            if (client.ClientId != refreshTicket.ClientId)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Attempt to authenticate with refresh ticket from other client. Request client id: {RequestClientId}, Ticket client id: {TicketClientId}",
                    client.ClientId,
                    refreshTicket.ClientId);
                return;
            }

            var result = await client.AuthenticateWithRefreshTicket(refreshTicket);
            if (result == null)
            {
                if (refreshTicket.UserId != null)
                {
                    SecurityLog.CreateRecord(
                        SecurityLog.EnType.AuthenticationDenied,
                        EnSeverity.Crucial,
                        context.OwinContext.GetRequestDescription(),
                        "Grant refresh_token denied for {ClientId} with user {UserId}",
                        client.ClientId,
                        refreshTicket.UserId);
                }
                else
                {
                    SecurityLog.CreateRecord(
                        SecurityLog.EnType.AuthenticationDenied,
                        EnSeverity.Crucial,
                        context.OwinContext.GetRequestDescription(),
                        "Grant refresh_token denied for {ClientId}",
                        client.ClientId);
                }

                return;
            }

            context.OwinContext.Set(OwinContextAuthenticationResultKey, result);
            var identity = new ClaimsIdentity(result.AccessTicket.User?.UserId ?? result.AccessTicket.ClientId);
            context.Validated(identity);

            if (refreshTicket.UserId != null)
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationGranted,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Grant refresh_token for {ClientId} with user {UserId} succeeded",
                    client.ClientId,
                    refreshTicket.UserId);
            }
            else
            {
                SecurityLog.CreateRecord(
                    SecurityLog.EnType.AuthenticationGranted,
                    EnSeverity.Crucial,
                    context.OwinContext.GetRequestDescription(),
                    "Grant refresh_token for {ClientId} succeeded",
                    client.ClientId);
            }
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
            var authenticationResult = context.OwinContext.Get<AuthenticationResult>(OwinContextAuthenticationResultKey);
            context.Properties.ExpiresUtc = authenticationResult.AccessTicket.Expiring;
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
            return Task.CompletedTask;
        }
    }
}
