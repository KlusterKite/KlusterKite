// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationProvider.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Provides methods for authentication
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Authentication
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;

    using Autofac;

    using KlusterKite.Security.Attributes;

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
        public AuthenticationProvider(IComponentContext container)
        {
            this.clientProviders = container.Resolve<IEnumerable<IClientProvider>>()
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
