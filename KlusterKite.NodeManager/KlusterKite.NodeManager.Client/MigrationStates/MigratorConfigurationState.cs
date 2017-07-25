// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorConfigurationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The state of migrators resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using System.Collections.Generic;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The state of migrators resources
    /// </summary>
    [ApiDescription("The state of migrators resources", Name = "MigratorConfigurationState")]
    public class MigratorConfigurationState
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        [DeclareField("the migrator type name", IsKey = true)]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the migrator name
        /// </summary>
        [DeclareField("the migrator name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the migrator defined migration points
        /// </summary>
        [DeclareField("the migrator defined migration points")]
        public List<string> MigrationPoints { get; set; }

        /// <summary>
        /// Gets or sets the last defined point for current configuration
        /// </summary>
        [DeclareField("the last defined point for current configuration")]
        public string LastDefinedPoint { get; set; }

        /// <summary>
        /// Gets or sets the state of defined resources
        /// </summary>
        [DeclareField("the state of defined resources")]
        public List<ResourceConfigurationState> Resources { get; set; }

        /// <summary>
        /// Gets or sets the type of resource dependency
        /// </summary>
        [DeclareField("the type of resource dependency")]
        public EnResourceDependencyType DependencyType { get; set; }

        /// <summary>
        /// Gets or sets the priority of execution. In case of migration, the migrators will be run in order of priority from largest to lowest
        /// </summary>
        [DeclareField("the priority of execution. In case of migration, the migrators will be run in order of priority from largest to lowest")]
        public decimal Priority { get; set; }
    }
}