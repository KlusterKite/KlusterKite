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

    using ClusterKit.Security.Client;

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
        public IEnumerable<string> OwnScope => new string[0];

        /// <inheritdoc />
        public Task<UserSession> AuthenticateUserAsync(string userName, string password)
        {
            return Task.FromResult<UserSession>(null);
        }

        /// <inheritdoc />
        public Task<UserSession> AuthenticateSelf()
        {
            // todo: move expiring timespan to config
            return Task.FromResult(new UserSession(null, null, this.ClientId, this.Type, this.OwnScope, DateTimeOffset.Now, DateTimeOffset.Now.AddHours(1), null));
        }
    }
}
