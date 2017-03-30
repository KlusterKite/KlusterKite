// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnConnectionAction.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   In case of applying to the connection property / method, indicates one or several connection actions to be applied to
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Attributes.Authorization
{
    using System;

    /// <summary>
    /// In case of applying to the connection property / method, indicates one or several connection actions to be applied to
    /// </summary>
    [Flags]
    public enum EnConnectionAction
    {
        /// <summary>
        /// Query the connection
        /// </summary>
        Query = 1,

        /// <summary>
        /// Creates a new node in the connection
        /// </summary>
        Create = 2,

        /// <summary>
        /// Updates the node in the connection
        /// </summary>
        Update = 4,

        /// <summary>
        /// Removes the node from the connection
        /// </summary>
        Delete = 8,

        /// <summary>
        /// Any connection action
        /// </summary>
        All = Query | Create | Update | Delete,

        /// <summary>
        /// Any modification action
        /// </summary>
        Modify = Create | Update | Delete,
    }
}