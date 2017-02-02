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
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Core;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

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
        public string Type => this.GetType().Name;

        /// <inheritdoc />
        public IEnumerable<string> OwnScope => new string[0];

        /// <inheritdoc />
        public async Task<UserSession> AuthenticateUserAsync(string userName, string password)
        {
            var response =
                await this.system.GetNodeManager()
                    .Ask<CrudActionResponse<User>>(
                        new AuthenticateUser { Login = userName, Password = password },
                        ConfigurationUtils.GetRestTimeout(this.system));

            if (response.Data != null)
            {
                var user = response.Data;
                // ReSharper disable ArrangeRedundantParentheses
                if (user.IsDeleted || !user.IsBlocked
                    || (user.ActiveTill.HasValue && user.ActiveTill.Value < this.system.Scheduler.Now)
                    || (user.BlockedTill.HasValue && user.BlockedTill.Value > this.system.Scheduler.Now))
                {
                    return null;
                }

                // ReSharper restore ArrangeRedundantParentheses
                return new UserSession(
                    user.GetDescription(),
                    user.GetScope(),
                    this.ClientId,
                    this.Type,
                    this.OwnScope,
                    this.system.Scheduler.Now,
                    this.system.Scheduler.Now.AddMinutes(30),
                    null);
            }

            return null;
        }

        /// <inheritdoc />
        public Task<UserSession> AuthenticateSelf()
        {
            return Task.FromResult<UserSession>(null);
        }
    }
}
