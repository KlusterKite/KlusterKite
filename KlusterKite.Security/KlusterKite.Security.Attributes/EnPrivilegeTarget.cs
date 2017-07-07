// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnPrivilegeTarget.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The target of privilege to apply
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
{
    using System;

    /// <summary>
    /// The target of privilege to apply
    /// </summary>
    [Flags]
    public enum EnPrivilegeTarget
    {
        /// <summary>
        /// The privilege target is undefined
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Clients (applications) can be granted with this privilege
        /// </summary>
        Client = 1,

        /// <summary>
        /// Users can be granted with this privilege
        /// </summary>
        User = 2,

        /// <summary>
        /// Both clients and users can be granted with this privilege
        /// </summary>
        ClientAndUser = 3
    }
}