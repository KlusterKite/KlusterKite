// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceMigrationState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The resource state according to the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.MigrationStates
{
    using ClusterKit.NodeManager.RemoteDomain;

    /// <summary>
    /// The resource state according to the migration
    /// </summary>
    public class ResourceMigrationState : ResourceReleaseState
    {
        /// <summary>
        /// Gets or sets the migration source position for this resource type
        /// </summary>
        public string SourcePoint { get; set; }

        /// <summary>
        /// Gets or sets the migration destination position for this resource type
        /// </summary>
        public string DestinationPoint { get; set; }

        /// <summary>
        /// Gets the resource current position
        /// </summary>
        public EnResourcePosition Position
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.CurrentPoint))
                {
                    return EnResourcePosition.NotCreated;
                }

                if (string.IsNullOrWhiteSpace(this.DestinationPoint))
                {
                    return EnResourcePosition.Obsolete;
                }

                if (this.SourcePoint == this.CurrentPoint && this.DestinationPoint == this.CurrentPoint)
                {
                    return EnResourcePosition.SourceAndDestination;
                }

                if (this.SourcePoint == this.CurrentPoint)
                {
                    return EnResourcePosition.Source;
                }

                if (this.DestinationPoint == this.CurrentPoint)
                {
                    return EnResourcePosition.Destination;
                }

                return EnResourcePosition.Undefined;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating what release can execute migration to source release point
        /// </summary>
        public EnMigrationSide? MigrationToSourceExecutor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what release can execute migration to destination release point
        /// </summary>
        public EnMigrationSide? MigrationToDestinationExecutor { get; set; }

        /// <summary>
        /// Creates <see cref="ResourceMigrationState"/>
        /// </summary>
        /// <param name="state">The migrator state</param>
        /// <param name="resource">The resource state</param>
        /// <param name="position">The migrator position</param>
        /// <returns>The <see cref="ResourceMigrationState"/></returns>
        public static ResourceMigrationState CreateFrom(
            MigratorReleaseState state,
            ResourceReleaseState resource,
            EnMigratorPosition position)
        {
            var sourcePoint = position == EnMigratorPosition.New ? null : state.LastDefinedPoint;
            var destinationPoint = position == EnMigratorPosition.Obsolete ? null : state.LastDefinedPoint;

            var migrationToSourceExecutor = resource.CurrentPoint == state.LastDefinedPoint
                                                ? null
                                                : position == EnMigratorPosition.Obsolete
                                                  && state.MigrationPoints.Contains(resource.CurrentPoint)
                                                    ? EnMigrationSide.Source
                                                    : (EnMigrationSide?)null;

            var migrationToDestinationExecutor = resource.CurrentPoint == state.LastDefinedPoint
                                                     ? null
                                                     : position == EnMigratorPosition.New
                                                       && state.MigrationPoints.Contains(resource.CurrentPoint)
                                                         ? EnMigrationSide.Destination
                                                         : (EnMigrationSide?)null;
            return new ResourceMigrationState
                       {
                           Name = resource.Name,
                           Code = resource.Code,
                           CurrentPoint = resource.CurrentPoint,
                           SourcePoint = sourcePoint,
                           DestinationPoint = destinationPoint,
                           MigrationToSourceExecutor = migrationToSourceExecutor,
                           MigrationToDestinationExecutor = migrationToDestinationExecutor
                       };
        }

        /// <summary>
        /// Creates <see cref="ResourceMigrationState"/>
        /// </summary>
        /// <param name="sourceMigratorState">
        /// The migrator state in source release
        /// </param>
        /// <param name="sourceState">
        /// The resource state in source release
        /// </param>
        /// <param name="destinationMigratorState">
        /// The migrator state in destination release
        /// </param>
        /// <param name="destinationState">
        /// The resource state in destination release
        /// </param>
        /// <returns>
        /// The <see cref="ResourceMigrationState"/>
        /// </returns>
        public static ResourceMigrationState CreateFrom(
            MigratorReleaseState sourceMigratorState,
            ResourceReleaseState sourceState,
            MigratorReleaseState destinationMigratorState,
            ResourceReleaseState destinationState)
        {
            var currentPoint = destinationState.CurrentPoint ?? sourceState.CurrentPoint;

            var migrationToSourceExecutor =
                destinationMigratorState.MigrationPoints.Contains(currentPoint)
                && destinationMigratorState.MigrationPoints.Contains(sourceMigratorState.LastDefinedPoint)
                    ? EnMigrationSide.Destination
                    : sourceMigratorState.MigrationPoints.Contains(currentPoint)
                        ? EnMigrationSide.Source
                        : (EnMigrationSide?)null;

            var migrationToDestinationExecutor = destinationMigratorState.MigrationPoints.Contains(currentPoint)
                                                     ? EnMigrationSide.Destination
                                                     : sourceMigratorState.MigrationPoints.Contains(currentPoint)
                                                       && sourceMigratorState.MigrationPoints.Contains(
                                                           destinationMigratorState.LastDefinedPoint)
                                                         ? EnMigrationSide.Source
                                                         : (EnMigrationSide?)null;

            return new ResourceMigrationState
                       {
                           Code = destinationState.Code,
                           Name = destinationState.Name,
                           SourcePoint = sourceMigratorState.LastDefinedPoint,
                           DestinationPoint = destinationMigratorState.LastDefinedPoint,
                           CurrentPoint = currentPoint,
                           MigrationToSourceExecutor =
                               currentPoint != sourceMigratorState.LastDefinedPoint
                                   ? migrationToSourceExecutor
                                   : null,
                           MigrationToDestinationExecutor =
                               currentPoint != destinationMigratorState.LastDefinedPoint
                                   ? migrationToDestinationExecutor
                                   : null
                       };
        }
    }
}
