// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserSession.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The authenticated user session
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// The authenticated user session
    /// </summary>
    public class UserSession
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserSession"/> class.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="userScope">
        /// The user scope.
        /// </param>
        /// <param name="clientId">
        /// The client id.
        /// </param>
        /// <param name="clientType">
        /// The client type.
        /// </param>
        /// <param name="clientScope">
        /// The client scope.
        /// </param>
        /// <param name="created">
        /// The created time.
        /// </param>
        /// <param name="expiring">
        /// The expiring time.
        /// </param>
        /// <param name="extraData">
        /// The extra data.
        /// </param>
        public UserSession([CanBeNull] IUser user, [CanBeNull]IEnumerable<string> userScope, [NotNull] string clientId, [NotNull] string clientType, [NotNull] IEnumerable<string> clientScope, DateTimeOffset created, DateTimeOffset expiring, object extraData)
        {
            this.User = user;
            this.UserScope = userScope;
            this.ClientId = clientId;
            this.ClientType = clientType;
            this.ClientScope = clientScope;
            this.Created = created;
            this.Expiring = expiring;
            this.ExtraData = extraData;
        }

        /// <summary>
        /// Gets the user identity
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public IUser User { get; }

        /// <summary>
        /// Gets the user's authorized actions scope
        /// </summary>
        [CanBeNull]
        [UsedImplicitly]
        public IEnumerable<string> UserScope { get; }

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
        /// Gets the client's authorized actions scope
        /// </summary>
        [NotNull]
        [UsedImplicitly]
        public IEnumerable<string> ClientScope { get; }

        /// <summary>
        /// Gets the session created time
        /// </summary>
        public DateTimeOffset Created { get; }

        /// <summary>
        /// Gets the session expiring time
        /// </summary>
        public DateTimeOffset? Expiring { get; }

        /// <summary>
        /// Gets some extra data from client
        /// </summary>
        [UsedImplicitly]
        public object ExtraData { get; }
    }
}