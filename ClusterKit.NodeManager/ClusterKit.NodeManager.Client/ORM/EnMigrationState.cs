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
    using ClusterKit.API.Attributes;

    /// <summary>
    /// The list of <see cref="Migration"/> states
    /// </summary>
    [ApiDescription("The list of migration states", Name = "EnMigrationState")]
    public enum EnMigrationState
    {
        /// <summary>
        /// The migration is in preparing state. The release compatibility check is in process
        /// </summary>
        [ApiDescription("The migration is in preparing state. The release compatibility check is in process")]
        Preparing,

        /// <summary>
        /// The migration is ready to run
        /// </summary>
        [ApiDescription("The migration is ready to run")]
        Ready,

        /// <summary>
        /// The migration failed
        /// </summary>
        [ApiDescription("The migration failed")]
        Failed,

        /// <summary>
        /// The migration is completed
        /// </summary>
        [ApiDescription("The migration is completed")]
        Completed,

        /// <summary>
        /// The migration is rollbacked
        /// </summary>
        /// TODO: this
        [ApiDescription("The migration is rollbacked")]
        Rollbacked
    }
}