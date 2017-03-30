// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserResetPasswordRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to change the user password
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using System;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The request to change the user password
    /// </summary>
    public class UserResetPasswordRequest
    {
        /// <summary>
        /// Gets or sets the user uid
        /// </summary>
        public Guid UserUid { get; set; }

        /// <summary>
        /// Gets or sets some extra data to return
        /// </summary>
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Gets or sets the user new password to set
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Request { get; set; }
    }
}