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

    using ClusterKit.API.Attributes;
    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// The state of resources of the migrator template
    /// </summary>
    [ApiDescription("The state of resources of the migrator template", Name = "MigratorTemplateReleaseState")]
    public class MigratorTemplateReleaseState
    {
        /// <summary>
        /// Gets or sets the template code
        /// </summary>
        [DeclareField("the template code", IsKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the migrator template
        /// </summary>
        [DeclareField("the migrator template")]
        public MigratorTemplate Template { get; set; }

        /// <summary>
        /// Gets or sets the list of migrator states
        /// </summary>
        [DeclareField("the migrator states")]
        public List<MigratorReleaseState> MigratorsStates { get; set; }
    }
}
