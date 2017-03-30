// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserChangePasswordRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to change the user password
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using Akka.Routing;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The request to change the user password
    /// </summary>
    public class UserChangePasswordRequest : IConsistentHashable
    {
        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets some extra data to return
        /// </summary>
        public byte[] ExtraData { get; set; }

        /// <summary>
        /// Gets or sets the user old password to check
        /// </summary>
        public string OldPassword { get; set; }

        /// <summary>
        /// Gets or sets the user new password to set
        /// </summary>
        public string NewPassword { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Request { get; set; }

        /// <inheritdoc />
        public object ConsistentHashKey => this.Login;
    }
}
