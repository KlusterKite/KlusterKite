// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthenticateUserWithCredentials.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to authenticate user
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages
{
    /// <summary>
    /// The request to authenticate user
    /// </summary>
    public class AuthenticateUserWithCredentials
    {
        /// <summary>
        /// Gets or sets the user login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Gets or sets the user password
        /// </summary>
        public string Password { get; set; }
    }
}
