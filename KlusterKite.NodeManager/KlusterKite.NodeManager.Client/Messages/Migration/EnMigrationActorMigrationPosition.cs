// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationActorMigrationPosition.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The list of possible resource migration position according to cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The list of possible resource migration position according to cluster migration
    /// </summary>
    [ApiDescription("The list of possible resource migration position according to cluster migration", Name = "EnMigrationActorMigrationPosition")]
    public enum EnMigrationActorMigrationPosition
    {
        /// <summary>
        /// All resources are at the source configuration position
        /// </summary>
        [ApiDescription("All resources are at the source configuration position")]
        Source = 1,

        /// <summary>
        /// All resources are at the destination configuration position
        /// </summary>
        [ApiDescription("All resources are at the destination configuration position")]
        Destination = 2,

        /// <summary>
        /// Some resources are at the source position, others are at destination. Also some resource can be between source and destination positions and can be migrated.
        /// </summary>
        [ApiDescription("Some resources are at the source position, others are at destination. Also some resource can be between source and destination positions and can be migrated.")]
        PartiallyMigrated,

        /// <summary>
        /// The two configurations doesn't require resource migration
        /// </summary>
        [ApiDescription("The two configurations doesn't require resource migration")]
        NoMigrationNeeded,

        /// <summary>
        /// There are resources that neither in source nor in destination position and can't be migrated automatically
        /// </summary>
        [ApiDescription("There are resources that neither in source nor in destination position and can't be migrated automatically")]
        Broken
    }
}