// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorTemplateReleaseState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ReleaseResourcesState type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.MigrationStates
{
    using System.Collections.Generic;

    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// The state of resources of the migrator template
    /// </summary>
    public class MigratorTemplateReleaseState
    {
        /// <summary>
        /// Gets or sets the migrator template
        /// </summary>
        public MigratorTemplate Template { get; set; }

        /// <summary>
        /// Gets or sets the list of migrator states
        /// </summary>
        public List<MigratorReleaseState> MigratorsStates { get; set; }
    }
}
