// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Provides methods for authentication
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Authentication
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;

    using Castle.Windsor;

    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides methods for authentication
    /// </summary>
    [UsedImplicitly]
    public class AuthenticationProvider
    {
        /// <summary>
        /// The client providers
        /// </summary>
        private readonly ImmutableList<IClientProvider> clientProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="container">
        /// The container.
        /// </param>
        public AuthenticationProvider(IWindsorContainer container)
        {
            this.clientProviders = container.ResolveAll<IClientProvider>()
                .OrderByDescending(c => c.Priority)
                .ToImmutableList();
        }

        /// <summary>
        /// Tries to acquire client by it's credentials
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <param name="clientSecret">The client secret</param>
        /// <returns>The client</returns>
        public async Task<IClient> GetClient(string clientId, string clientSecret)
        {
            foreach (var clientProvider in this.clientProviders)
            {
                var client = await clientProvider.GetClientAsync(clientId, clientSecret);
                if (client != null)
                {
                    return client;
                }
            }

            return null;
        }
    }
}
