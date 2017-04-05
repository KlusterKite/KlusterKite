// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClientProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Provides API for oAuth 2 authentication to get and check clients (client applications)
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace ClusterKit.Security.Attributes
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides API for oAuth 2 authentication to get and check clients (client applications)
    /// </summary>
    public interface IClientProvider
    {
        /// <summary>
        /// Gets the priority value of the provider.
        /// Providers will be checked in the priority order from largest to lowest
        /// </summary>
        decimal Priority { get; }

        /// <summary>
        /// Gets the client from authentication data
        /// </summary>
        /// <param name="clientId">The client id</param>
        /// <param name="secret">The client secret (can be null for public applications)</param>
        /// <returns>The registered client or null</returns>
        Task<IClient> GetClientAsync(string clientId, string secret);
    }
}