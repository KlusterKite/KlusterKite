// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationDirection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The direction of cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    /// <summary>
    /// The direction of cluster migration
    /// </summary>
    public enum EnMigrationDirection
    {
        /// <summary>
        /// The cluster is upgrading
        /// </summary>
        Upgrade,

        /// <summary>
        /// The cluster is downgrading
        /// </summary>
        Downgrade,

        /// <summary>
        /// The cluster resources don't need upgrade. They are up to date.
        /// </summary>
        Stay,

        /// <summary>
        /// The migration direction can-not be evaluated. The migration is corrupted.
        /// </summary>
        Undefined
    }
}