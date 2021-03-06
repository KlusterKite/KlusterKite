﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IClient.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The description of the client applications
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// The description of the client applications
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Gets the client application id
        /// </summary>
        string ClientId { get; }

        /// <summary>
        /// Gets the client application name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the client type name
        /// </summary>
        string Type { get; }

        /// <summary>
        /// Gets the authorization scope for application
        /// </summary>
        IEnumerable<string> OwnScope { get; }

        /// <summary>
        /// Authenticate user using its login and password
        /// </summary>
        /// <param name="userName">The user name (login)</param>
        /// <param name="password">The user password</param>
        /// <returns>The authenticated user or null</returns>
        Task<AuthenticationResult> AuthenticateUserAsync(string userName, string password);

        /// <summary>
        /// The application authenticates itself and will make operations on it's own behalf.
        /// </summary>
        /// <returns>The generated user session or null, in case self authentication is denied</returns>
        Task<AuthenticationResult> AuthenticateSelf();

        /// <summary>
        /// Renews authentication session
        /// </summary>
        /// <param name="refreshTicket">The refresh ticket data</param>
        /// <returns>The subsequent authentication result</returns>
        Task<AuthenticationResult> AuthenticateWithRefreshTicket(RefreshTicket refreshTicket);
    }
}
