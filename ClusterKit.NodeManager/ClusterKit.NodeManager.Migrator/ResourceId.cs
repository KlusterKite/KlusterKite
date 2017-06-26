// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceId.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of some migratable resource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator
{
    using System;

    /// <summary>
    /// The description of some migratable resource
    /// </summary>
    public class ResourceId
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
        /// Gets or sets the resource connection string (or some identification to help connect to such resource)
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the resource provider name
        /// </summary>
        public string ProviderName { get; set; }
    }
}