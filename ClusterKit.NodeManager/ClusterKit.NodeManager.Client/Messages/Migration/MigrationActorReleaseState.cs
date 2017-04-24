// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorReleaseState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="MigrationActor" /> state with no active migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using ClusterKit.NodeManager.Client.MigrationStates;

    /// <summary>
    /// The MigrationActor state with no active migration
    /// </summary> 
    public class MigrationActorReleaseState
    {
        /// <summary>
        /// Gets or sets the list of template states
        /// </summary>
        public List<MigratorTemplateReleaseState> States { get; set; }
    }
}
