// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TokenController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the TokenController type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System;
    using System.Threading.Tasks;

    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization;

    using Microsoft.AspNetCore.Mvc;

    using Newtonsoft.Json;

    /// <summary>
    /// Processing authentication requests
    /// </summary>
    [Route("/api/1.x/security/token")]
    public class TokenController : Controller
    {
        /// <summary>
        /// The authentication provider
        /// </summary>
        private readonly AuthenticationProvider authenticationProvider;

        /// <summary>
        /// The token manager.
        /// </summary>
        private readonly ITokenManager tokenManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenController"/> class.
        /// </summary>
        /// <param name="authenticationProvider">
        /// The authentication provider.
        /// </param>
        /// <param name="tokenManager">The token manager</param>
        public TokenController(AuthenticationProvider authenticationProvider, ITokenManager tokenManager)
        {
            this.authenticationProvider = authenticationProvider;
            this.tokenManager = tokenManager;
        }

        /// <summary>
        /// Processes the grant requests
        /// </summary>
        /// <returns>The token processing result</returns>
        [HttpPost]
        public Task<IActionResult> Grant()
        {
            var grantType = this.GetRequestParameter("grant_type");
            switch (grantType)
            {
                case "password":
                    return this.GrantPassword(
                        this.GetRequestParameter("username"),
                        this.GetRequestParameter("password", false),
                        this.GetRequestParameter("client_id"),
                        this.GetRequestParameter("client_secret", false));
                case "refresh_token":
                    return this.RefreshToken(
                        this.GetRequestParameter("refresh_token", false),
                        this.GetRequestParameter("client_id"),
                        this.GetRequestParameter("client_secret", false));
                case "client_credentials":
                    return this.ClientCredentials(
                        this.GetRequestParameter("client_id"),
                        this.GetRequestParameter("client_secret", false));
            }

            return Task.FromResult<IActionResult>(this.BadRequest());
        }

        /// <summary>
        /// Processes the refresh token request
        /// </summary>
        /// <param name="refreshToken">
        /// The refresh token.
        /// </param>
        /// <param name="clientId">The client application id</param>
        /// <param name="clientSecret">The client application password</param>
        /// <returns>The result of request process</returns>
        private async Task<IActionResult> RefreshToken(string refreshToken, string clientId, string clientSecret)
        {
            var token = await this.tokenManager.ValidateRefreshToken(refreshToken);
            if (token == null)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    "Refresh token check failed",
                    clientId);
                return this.BadRequest();
            }

            if (token.ClientId != clientId)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    "Attempt to authenticate with refresh token from other client id",
                    clientId);
                return this.BadRequest();
            }

            var client = await this.authenticationProvider.GetClient(clientId, clientSecret);
            if (client == null)
            {
                var message = string.IsNullOrWhiteSpace(clientSecret)
                                  ? "Failed authentication for client {ClientId} without secret"
                                  : "Failed authentication for client {ClientId} with secret";
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    message,
                    clientId);
                return this.BadRequest();
            }

            var result = await client.AuthenticateWithRefreshTicket(token);
            if (result == null)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    "Authentication with refresh token failed",
                    clientId);
                return this.BadRequest();
            }

            var accessToken = await this.tokenManager.CreateAccessToken(result.AccessTicket);
            refreshToken = await this.tokenManager.CreateRefreshToken(result.RefreshTicket);

            var response = new TokenResponse
                               {
                                   AccessToken = accessToken,
                                   RefreshToken = refreshToken,
                                   TokenType = "bearer",
                                   Expires = result.AccessTicket.Expiring.HasValue
                                                 ? (int?)(result.AccessTicket.Expiring.Value
                                                          - DateTimeOffset.Now).TotalSeconds
                                                 : null
                               };

            return this.Json(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// Processes the grant client credentials request
        /// </summary>
        /// <param name="clientId">The client application id</param>
        /// <param name="clientSecret">The client application password</param>
        /// <returns>The result of request process</returns>
        private async Task<IActionResult> ClientCredentials(
            string clientId,
            string clientSecret)
        {
            var client = await this.authenticationProvider.GetClient(clientId, clientSecret);
            if (client == null)
            {
                var message = string.IsNullOrWhiteSpace(clientSecret)
                                  ? "Failed authentication for client {ClientId} without secret"
                                  : "Failed authentication for client {ClientId} with secret";
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    message,
                    clientId);
                return this.BadRequest();
            }

            var result = await client.AuthenticateSelf();
            if (result?.AccessTicket == null)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    "Grant password failed for client {ClientId}",
                   client.ClientId);
                return this.BadRequest();
            }

            var accessToken = await this.tokenManager.CreateAccessToken(result.AccessTicket);
            var refreshToken = result.RefreshTicket != null 
                ? await this.tokenManager.CreateRefreshToken(result.RefreshTicket) 
                : null;

            var response = new TokenResponse
                               {
                                   AccessToken = accessToken,
                                   RefreshToken = refreshToken,
                                   TokenType = "bearer",
                                   Expires = result.AccessTicket.Expiring.HasValue
                                                 ? (int?)(result.AccessTicket.Expiring.Value
                                                          - DateTimeOffset.Now).TotalSeconds
                                                 : null
                               };

            return this.Json(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// Processes the grant password request
        /// </summary>
        /// <param name="userName">The user name</param>
        /// <param name="password">The user password</param>
        /// <param name="clientId">The client application id</param>
        /// <param name="clientSecret">The client application password</param>
        /// <returns>The result of request process</returns>
        private async Task<IActionResult> GrantPassword(
            string userName,
            string password,
            string clientId,
            string clientSecret)
        {
            var client = await this.authenticationProvider.GetClient(clientId, clientSecret);
            if (client == null)
            {
                var message = string.IsNullOrWhiteSpace(clientSecret)
                                  ? "Failed authentication for client {ClientId} without secret"
                                  : "Failed authentication for client {ClientId} with secret";
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    message,
                    clientId);
                return this.BadRequest();
            }

            var result = await client.AuthenticateUserAsync(userName, password);
            if (result?.AccessTicket.User == null)
            {
                SecurityLog.CreateRecord(
                    EnSecurityLogType.AuthenticationDenied,
                    EnSeverity.Crucial,
                    this.GetRequestDescription(),
                    "Grant password failed for user {UserName} with client {ClientId}",
                    userName,
                    client.ClientId);
                return this.BadRequest();
            }

            var accessToken = await this.tokenManager.CreateAccessToken(result.AccessTicket);
            var refreshToken = await this.tokenManager.CreateRefreshToken(result.RefreshTicket);

            var response = new TokenResponse
                               {
                                   AccessToken = accessToken,
                                   RefreshToken = refreshToken,
                                   TokenType = "bearer",
                                   Expires = result.AccessTicket.Expiring.HasValue
                                                 ? (int?)(result.AccessTicket.Expiring.Value
                                                          - DateTimeOffset.Now).TotalSeconds
                                                 : null
                               };

            return this.Json(response, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        /// <summary>
        /// Gets the parameter from the request
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <param name="allowGet">A value indicating whether parameter is allowed to be passed via query</param>
        /// <returns>The parameter value</returns>
        private string GetRequestParameter(string parameterName, bool allowGet = true)
        {
            var parameter = this.HttpContext.Request.Form[parameterName].ToString();
            if (string.IsNullOrWhiteSpace(parameter) && allowGet)
            {
                parameter = this.HttpContext.Request.Query[parameterName].ToString();
            }

            return string.IsNullOrWhiteSpace(parameter) ? null : parameter;
        }
    }
}
