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
        Downgrade
    }
}