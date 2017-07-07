// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorReleaseState.cs" company="KlusterKite">
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

    /// <summary>
    /// The state of migrators resources
    /// </summary>
    [ApiDescription("The state of migrators resources", Name = "MigratorReleaseState")]
    public class MigratorReleaseState
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
        /// Gets or sets the last defined point for current release
        /// </summary>
        [DeclareField("the last defined point for current release")]
        public string LastDefinedPoint { get; set; }

        /// <summary>
        /// Gets or sets the state of defined resources
        /// </summary>
        [DeclareField("the state of defined resources")]
        public List<ResourceReleaseState> Resources { get; set; }
    }
}