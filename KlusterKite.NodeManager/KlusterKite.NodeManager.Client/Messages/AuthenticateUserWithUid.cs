// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticateUserWithUid.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The request to authenticate user with it's uid
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages
{
    using System;

    using Akka.Routing;

    /// <summary>
    /// The request to authenticate user with it's uid
    /// </summary>
    public class AuthenticateUserWithUid : IConsistentHashable
    {
        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        public Guid Uid { get; set; }

        /// <inheritdoc />
        public object ConsistentHashKey => this.Uid;
    }
}