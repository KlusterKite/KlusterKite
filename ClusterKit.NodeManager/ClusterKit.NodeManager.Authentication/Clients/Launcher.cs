// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Launcher.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The launcher program, that configures and starts individual nodes
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Authentication.Clients
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.NodeManager.Client;
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// The launcher program, that configures and starts individual nodes
    /// </summary>
    public class Launcher : IClient
    {
        /// <summary>
        /// The web application client id
        /// </summary>
        public const string LauncherClientId = "ClusterKit.NodeManager.Launcher";

        /// <inheritdoc />
        public string ClientId => LauncherClientId;

        /// <inheritdoc />
        public string Name => "The launcher program, that configures and starts individual nodes";

        /// <inheritdoc />
        public string Type => this.GetType().Name;

        /// <inheritdoc />
        public IEnumerable<string> OwnScope => new[]
                                                   {
                                                       Privileges.GetConfiguration
                                                   };

        /// <inheritdoc />
        public Task<AuthenticationResult> AuthenticateUserAsync(string userName, string password)
        {
            return Task.FromResult<AuthenticationResult>(null);
        }

        /// <inheritdoc />
        public Task<AuthenticationResult> AuthenticateSelf()
        {
            var result = new AuthenticationResult(
                new AccessTicket(null, null, this.ClientId, this.Type, this.OwnScope, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), null),
                null);

            // todo: move expiring timespan to config
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<AuthenticationResult> AuthenticateWithRefreshTicket(RefreshTicket refreshTicket)
        {
            return Task.FromResult<AuthenticationResult>(null);
        }
    }
}
