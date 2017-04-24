// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigratorPosition.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="IMigrator" /> or <see cref="MigratingTemplate" /> position in the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.MigrationStates
{
    using ClusterKit.NodeManager.Migrator;

    /// <summary>
    /// The <see cref="IMigrator"/> or <see cref="MigratorTemplateMigrationState"/> position in the migration
    /// </summary>
    public enum EnMigratorPosition
    {
        /// <summary>
        /// This entity was introduce in the destination release
        /// </summary>
        New,

        /// <summary>
        /// This entity is located in both source and destination releases
        /// </summary>
        Merged,

        /// <summary>
        /// This entity is not present in the destination release
        /// </summary>
        Obsolete
    }
}