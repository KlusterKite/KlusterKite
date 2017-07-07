// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigratorPosition.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="IMigrator" /> or <see cref="MigratingTemplate" /> position in the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The <see cref="IMigrator"/> or <see cref="MigratorTemplateMigrationState"/> position in the migration
    /// </summary>
    [ApiDescription("The IMigrator or MigratorTemplateMigrationState position in the migration", Name = "EnMigratorPosition")]
    public enum EnMigratorPosition
    {
        /// <summary>
        /// This entity was introduce in the destination release
        /// </summary>
        [ApiDescription("This entity was introduce in the destination release")]
        New,

        /// <summary>
        /// This entity is located in both source and destination releases
        /// </summary>
        [ApiDescription("This entity is located in both source and destination releases")]
        Merged,

        /// <summary>
        /// This entity is not present in the destination release
        /// </summary>
        [ApiDescription("This entity is not present in the destination release")]
        Obsolete
    }
}