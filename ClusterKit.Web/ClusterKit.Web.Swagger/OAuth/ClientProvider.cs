// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generates Swagger application for dev environment
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Swagger.OAuth
{
    using System.Threading.Tasks;

    using Akka.Configuration;

    using ClusterKit.Security.Client;

    /// <summary>
    /// Generates Swagger application for development environment
    /// </summary>
    public class ClientProvider : IClientProvider
    {
        /// <summary>
        /// The system config
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientProvider"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public ClientProvider(Config config)
        {
            this.config = config;
        }

        /// <inheritdoc />
        public decimal Priority => 0M;

        /// <inheritdoc />
        public Task<IClient> GetClientAsync(string clientId, string secret)
        {
            if (clientId == this.config.GetString("ClusterKit.Web.Swagger.OAuth2ClientId")
                && this.config.GetBoolean("ClusterKit.Web.Swagger.EnableSwaggerApplication"))
            {
                return Task.FromResult<IClient>(new SwaggerClient(clientId));
            }

            return Task.FromResult<IClient>(null);
        }
    }
}
