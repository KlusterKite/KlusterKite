// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationSide.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The type of release in the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.MigrationStates
{
    /// <summary>
    /// The type of release in the migration
    /// </summary>
    public enum EnMigrationSide
    {
        /// <summary>
        /// The source release
        /// </summary>
        Source,

        /// <summary>
        /// The destination release
        /// </summary>
        Destination
    }
}