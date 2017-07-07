// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationSide.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The type of release in the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The type of release in the migration
    /// </summary>
    [ApiDescription("The type of release in the migration", Name = "EnMigrationSide")]
    public enum EnMigrationSide
    {
        /// <summary>
        /// The source release
        /// </summary>
        [ApiDescription("The source release")]
        Source,

        /// <summary>
        /// The destination release
        /// </summary>
        [ApiDescription("The destination release")]
        Destination
    }
}