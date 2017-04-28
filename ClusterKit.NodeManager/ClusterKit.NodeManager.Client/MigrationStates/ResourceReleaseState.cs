// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceReleaseState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The resource description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.MigrationStates
{
    using System;

    using ClusterKit.API.Attributes;

    /// <summary>
    /// The resource description
    /// </summary>
    [ApiDescription("The resource description", Name = "ResourceReleaseState")]
    [Serializable]
    public class ResourceReleaseState
    {
        /// <summary>
        /// Gets or sets the human readable resource name
        /// </summary>
        [DeclareField("the human readable resource name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource identification
        /// </summary>
        [DeclareField("the resource identification", IsKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the current migration point of the resource
        /// </summary>
        [DeclareField("the current migration point of the resource")]
        public string CurrentPoint { get; set; }
    }
}
