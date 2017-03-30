// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserRoleChangeRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Request to change user role membership
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using System;

    using ClusterKit.Security.Client;

    /// <summary>
    /// Request to change user role membership
    /// </summary>
    public abstract class UserRoleChangeRequest
    {
        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Gets or sets the role uid
        /// </summary>
        public Guid RoleUid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the response will be enriched with mutated user. Otherwise it will be mutated role.
        /// </summary>
        public bool ReturnUser { get; set; }

        /// <summary>
        /// Gets or sets some extra data to return
        /// </summary>
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Request { get; set; }
    }
}