// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceReleaseState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The resource description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.MigrationStates
{
    using System;

    /// <summary>
    /// The resource description
    /// </summary>
    public class ResourceReleaseState : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the human readable resource name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource identification
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the current migration point of the resource
        /// </summary>
        public string CurrentPoint { get; set; }
    }
}
