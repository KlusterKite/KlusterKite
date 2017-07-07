// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebApplication.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The ClusterKit nodmanager web UI
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.NodeManager.Authentication.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// The ClusterKit node manager web UI
    /// </summary>
    public class WebApplication : IClient
    {
        /// <summary>
        /// The web application client id
        /// </summary>
        public const string WebApplicationClientId = "ClusterKit.NodeManager.WebApplication";

        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem system;

        /// <inheritdoc />
        public WebApplication(ActorSystem system)
        {
            this.system = system;
        }

        /// <inheritdoc />
        public string ClientId => WebApplicationClientId;

        /// <inheritdoc />
        public string Name => "The ClusterKit nodmanager web UI";

        /// <inheritdoc />
        public IEnumerable<string> OwnScope => new string[0];

        /// <inheritdoc />
        public string Type => this.GetType().Name;

        /// <inheritdoc />
        public Task<AuthenticationResult> AuthenticateSelf()
        {
            return Task.FromResult<AuthenticationResult>(null);
        }

        /// <inheritdoc />
        public async Task<AuthenticationResult> AuthenticateUserAsync(string userName, string password)
        {
            var response =
                await this.system.GetNodeManager()
                    .Ask<CrudActionResponse<User>>(
                        new AuthenticateUserWithCredentials { Login = userName, Password = password },
                        ConfigurationUtils.GetRestTimeout(this.system));

            return response.Data != null ? this.CreateUserAuthenticationResult(response.Data) : null;
        }

        /// <inheritdoc />
        public async Task<AuthenticationResult> AuthenticateWithRefreshTicket(RefreshTicket refreshTicket)
        {
            Guid uid;
            if (!Guid.TryParseExact(refreshTicket.UserId, "N", out uid))
            {
                return null;
            }

            var response =
                await this.system.GetNodeManager()
                    .Ask<CrudActionResponse<User>>(
                        new AuthenticateUserWithUid { Uid = uid },
                        ConfigurationUtils.GetRestTimeout(this.system));

            return response.Data != null ? this.CreateUserAuthenticationResult(response.Data) : null;
        }

        /// <summary>
        /// Creates <see cref="ClusterKit.Security.Attributes.AuthenticationResult"/> from user data
        /// </summary>
        /// <param name="user">The user</param>
        /// <returns>The <see cref="ClusterKit.Security.Attributes.AuthenticationResult"/></returns>
        private AuthenticationResult CreateUserAuthenticationResult(User user)
        {
            // ReSharper disable ArrangeRedundantParentheses
            if (user.IsDeleted || user.IsBlocked
                || (user.ActiveTill.HasValue && user.ActiveTill.Value < this.system.Scheduler.Now)
                || (user.BlockedTill.HasValue && user.BlockedTill.Value > this.system.Scheduler.Now))
            {
                return null;
            }

            // ReSharper restore ArrangeRedundantParentheses
            // todo: move expiring time-span to config
            var accessTicket = new AccessTicket(
                user.GetDescription(),
                user.GetScope(),
                this.ClientId,
                this.Type,
                this.OwnScope,
                this.system.Scheduler.Now,
                this.system.Scheduler.Now.AddMinutes(30),
                null);
            var refreshTicket = new RefreshTicket(
                user.Uid.ToString("N"),
                this.ClientId,
                this.Type,
                this.system.Scheduler.Now,
                this.system.Scheduler.Now.AddDays(1));

            return new AuthenticationResult(accessTicket, refreshTicket);
        }
    }
}