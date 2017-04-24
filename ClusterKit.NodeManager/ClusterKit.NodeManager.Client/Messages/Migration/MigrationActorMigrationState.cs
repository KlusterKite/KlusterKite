// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorMigrationState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The notification of current migration check complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using ClusterKit.NodeManager.Client.MigrationStates;

    /// <summary>
    /// The notification of current migration check complete
    /// </summary>
    public class MigrationActorMigrationState
    {
        /// <summary>
        /// Gets or sets the current migration position
        /// </summary>
        public EnMigrationActorMigrationPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the list of migration states
        /// </summary>
        public List<MigratorTemplateMigrationState> TemplateStates { get; set; }
    }
}
