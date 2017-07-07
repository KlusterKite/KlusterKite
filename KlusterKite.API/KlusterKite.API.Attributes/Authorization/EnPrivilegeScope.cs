// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnPrivilegeScope.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The scope to look for privilege
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Attributes.Authorization
{
    /// <summary>
    /// The scope to look for privilege
    /// </summary>
    public enum EnPrivilegeScope
    {
        /// <summary>
        /// Access will be granted if user was granted with specified privilege
        /// </summary>
        User,

        /// <summary>
        /// Access will be granted if client application was granted with specified privilege
        /// </summary>
        Client,

        /// <summary>
        /// Access will be granted if user AND client application were granted with specified privilege
        /// </summary>
        Both,

        /// <summary>
        /// Access will be granted if user OR client application were granted with specified privilege
        /// </summary>
        /// <remarks>This is the default value</remarks>
        Any
    }
}