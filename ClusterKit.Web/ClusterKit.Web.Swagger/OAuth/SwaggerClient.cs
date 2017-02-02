// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SwaggerClient.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The virtual swagger client
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.OAuth
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The virtual swagger client
    /// </summary>
    public class SwaggerClient : IClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerClient"/> class.
        /// </summary>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        public SwaggerClient(string clientId)
        {
            this.ClientId = clientId;
        }

        /// <inheritdoc />
        public string ClientId { get; }

        /// <inheritdoc />
        public string Name => "Swagger UI";

        /// <inheritdoc />
        public string Type => "SwaggerClient";

        /// <inheritdoc />
        public IEnumerable<string> OwnScope => new[]
                                                   {
                                                       Privileges.TestGrantAll
                                                   };

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
