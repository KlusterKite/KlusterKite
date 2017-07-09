// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationSide.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The type of configuration in the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The type of configuration in the migration
    /// </summary>
    [ApiDescription("The type of configuration in the migration", Name = "EnMigrationSide")]
    public enum EnMigrationSide
    {
        /// <summary>
        /// The source configuration
        /// </summary>
        [ApiDescription("The source configuration")]
        Source,

        /// <summary>
        /// The destination configuration
        /// </summary>
        [ApiDescription("The destination configuration")]
        Destination
    }
}