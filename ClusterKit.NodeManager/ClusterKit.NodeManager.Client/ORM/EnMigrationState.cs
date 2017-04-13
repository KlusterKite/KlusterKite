// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of <see cref="Migration" /> states
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    /// <summary>
    /// The list of <see cref="Migration"/> states
    /// </summary>
    public enum EnMigrationState
    {
        /// <summary>
        /// The migration is in preparing state. The release compatibility check is in process
        /// </summary>
        Preparing,

        /// <summary>
        /// The migration is ready to run
        /// </summary>
        Ready,

        /// <summary>
        /// The migrator operations are running
        /// </summary>
        MigratingResources,

        /// <summary>
        /// The cluster nodes upgrade is in process
        /// </summary>
        MigratingNodes,

        /// <summary>
        /// The migrator operations are rollbacked
        /// </summary>
        RollbackingResources,

        /// <summary>
        /// The cluster node downgrade is in process
        /// </summary>
        RollbackingNodes,

        /// <summary>
        /// The migration is completed
        /// </summary>
        Completed,

        /// <summary>
        /// The migration is rollbacked
        /// </summary>
        Rollbacked
    }
}