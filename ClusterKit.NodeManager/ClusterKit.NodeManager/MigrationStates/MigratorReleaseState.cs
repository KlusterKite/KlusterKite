// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorReleaseState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The state of migrators resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.MigrationStates
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The state of migrators resources
    /// </summary>
    public class MigratorReleaseState : MarshalByRefObject
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the migrator name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the migrator defined migration points
        /// </summary>
        public List<string> MigrationPoints { get; set; }

        /// <summary>
        /// Gets or sets the last defined point for current release
        /// </summary>
        public string LastDefinedPoint { get; set; }

        /// <summary>
        /// Gets or sets the state of defined resources
        /// </summary>
        public List<ResourceReleaseState> Resources { get; set; }
    }
}