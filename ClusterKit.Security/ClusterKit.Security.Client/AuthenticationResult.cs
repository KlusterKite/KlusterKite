// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticationResult.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The result of the authentication procedure
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using JetBrains.Annotations;

    /// <summary>
    /// The result of the authentication procedure
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationResult"/> class.
        /// </summary>
        /// <param name="accessTicket">
        /// The access ticket.
        /// </param>
        /// <param name="refreshTicket">
        /// The refresh ticket.
        /// </param>
        public AuthenticationResult([NotNull]AccessTicket accessTicket, [CanBeNull]RefreshTicket refreshTicket)
        {
            this.AccessTicket = accessTicket;
            this.RefreshTicket = refreshTicket;
        }

        /// <summary>
        /// Gets the access ticket
        /// </summary>
        [NotNull]
        public AccessTicket AccessTicket { get;  }

        /// <summary>
        /// Gets the refresh token ticket
        /// </summary>
        [CanBeNull]
        public RefreshTicket RefreshTicket { get;  }
    }
}
