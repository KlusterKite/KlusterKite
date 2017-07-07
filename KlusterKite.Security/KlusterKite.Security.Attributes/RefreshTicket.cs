// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RefreshTicket.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The unique data to identify the user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Security.Attributes
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// The unique data to identify the user
    /// </summary>
    public class RefreshTicket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTicket"/> class.
        /// </summary>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="clientType">
        /// The client type.
        /// </param>
        /// <param name="created">
        /// The created time.
        /// </param>
        /// <param name="expiring">
        /// The expiring time.
        /// </param>
        public RefreshTicket(string userId, [NotNull] string clientId, [NotNull] string clientType, DateTimeOffset created, DateTimeOffset? expiring)
        {
            this.UserId = userId;
            this.ClientId = clientId;
            this.ClientType = clientType;
            this.Created = created;
            this.Expiring = expiring;
        }

        /// <summary>
        /// Gets the string representation of the user id (login or other representation)
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets the client id
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string ClientId { get; }

        /// <summary>
        /// Gets the client type
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public string ClientType { get; }

        /// <summary>
        /// Gets the session created time
        /// </summary>
        public DateTimeOffset Created { get; }

        /// <summary>
        /// Gets the session expiring time
        /// </summary>
        public DateTimeOffset? Expiring { get; }
    }
}
