// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnClusterMigrationOperationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of operation states
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The list of operation states
    /// </summary>
    [ApiDescription("The cluster migration operation states", Name = "EnClusterMigrationOperationState")]
    public enum EnClusterMigrationOperationState
    {
        /// <summary>
        /// The operation waits for launch
        /// </summary>
        [ApiDescription("The operation waits for launch")]
        Waiting = 1,

        /// <summary>
        /// The operation is running
        /// </summary>
        [ApiDescription("The operation is running")]
        InProgress = 2,

        /// <summary>
        /// The operation successfully completed
        /// </summary>
        [ApiDescription("The operation successfully completed")]
        Completed = 3,

        /// <summary>
        /// The operation failed
        /// </summary>
        [ApiDescription("The operation failed")]
        Faulted = 4,

        /// <summary>
        /// The operation was aborted
        /// </summary>
        [ApiDescription("The operation was aborted")]
        Aborted = 5
    }
}