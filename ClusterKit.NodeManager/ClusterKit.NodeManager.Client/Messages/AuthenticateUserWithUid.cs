// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticateUserWithUid.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to authenticate user with it's uid
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages
{
    using System;

    /// <summary>
    /// The request to authenticate user with it's uid
    /// </summary>
    public class AuthenticateUserWithUid
    {
        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        public Guid Uid { get; set; }
    }
}