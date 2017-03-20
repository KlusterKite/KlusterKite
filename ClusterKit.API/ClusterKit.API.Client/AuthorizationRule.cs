// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AuthorizationRule.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   A directive to describe authorization requirements
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using ClusterKit.API.Client.Attributes.Authorization;

    /// <summary>
    /// A directive to describe authorization requirements
    /// </summary>
    public class AuthorizationRule
    {
        /// <summary>
        /// Gets or sets a value indicating whether the connection action type name should be added to required privilege on authorization check
        /// </summary>
        public bool AddActionNameToRequiredPrivilege { get; set; }

        /// <summary>
        /// Gets or sets the required privilege
        /// </summary>
        public string Privilege { get; set; }

        /// <summary>
        /// Gets or sets the scope to look for privilege
        /// </summary>
        public EnPrivilegeScope Scope { get; set; } = EnPrivilegeScope.Any;

        /// <summary>
        /// Gets or sets the scope to look for privilege
        /// </summary>
        public EnConnectionAction ConnectionActions { get; set; } = EnConnectionAction.All;

        /// <summary>
        /// Gets or sets a value indicating whether that this rule will be ignored in case of valid user authentication session
        /// </summary>
        public bool IgnoreOnUserPresent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether that this rule will be ignored in case of request without user authentication session 
        /// Client application makes calls on it's on behalf
        /// </summary>
        public bool IgnoreOnUserNotPresent { get; set; }
    }
}
