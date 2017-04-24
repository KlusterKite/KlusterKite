// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationActorMigrationPosition.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The list of possible resource migration position according to cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    /// <summary>
    /// The list of possible resource migration position according to cluster migration
    /// </summary>
    public enum EnMigrationActorMigrationPosition
    {
        /// <summary>
        /// All resources are at the source release position
        /// </summary>
        Source = 1,

        /// <summary>
        /// All resources are at the destination release position
        /// </summary>
        Destination = 2,

        /// <summary>
        /// Some resources are at the source position, others are at destination. Also some resource can be between source and destination positions and can be migrated.
        /// </summary>
        PartiallyMigrated,

        /// <summary>
        /// The two releases doesn't require resource migration
        /// </summary>
        NoMigrationNeeded,

        /// <summary>
        /// There are resources that neither in source nor in destination position and can't be migrated automatically
        /// </summary>
        Broken
    }
}