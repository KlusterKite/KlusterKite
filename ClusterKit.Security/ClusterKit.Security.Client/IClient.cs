// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClient.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of the client applications
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;

    /// <summary>
    /// The description of the client applications
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Gets the client id
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the client name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the authorization scope for application
        /// </summary>
        IEnumerable<string> Scope { get; }

        /// <summary>
        /// Authenticate user using its login and password
        /// </summary>
        /// <param name="userName">The user name (login)</param>
        /// <param name="password">The user password</param>
        /// <returns>The authenticated user or null</returns>
        Task<IIdentity> AuthenticateUserAsync(string userName, string password);
    }
}
