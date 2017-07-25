// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceMigrationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The resource state according to the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The resource state according to the migration
    /// </summary>
    [ApiDescription("The resource state according to the migration", Name = "ResourceMigrationState")]
    public class ResourceMigrationState : ResourceConfigurationState
    {
        /// <summary>
        /// Gets or sets the migration source position for this resource type
        /// </summary>
        [DeclareField("the migration source position for this resource type")]
        public string SourcePoint { get; set; }

        /// <summary>
        /// Gets or sets the migration destination position for this resource type
        /// </summary>
        [DeclareField("the migration destination position for this resource type")]
        public string DestinationPoint { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what configuration can execute migration to source configuration point
        /// </summary>
        [DeclareField("a value indicating what configuration can execute migration to source configuration point")]
        public EnMigrationSide? MigrationToSourceExecutor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what configuration can execute migration to destination configuration point
        /// </summary>
        [DeclareField("a value indicating what configuration can execute migration to destination configuration point")]
        public EnMigrationSide? MigrationToDestinationExecutor { get; set; }

        /// <summary>
        /// Creates <see cref="ResourceMigrationState"/>
        /// </summary>
        /// <param name="templateCode">The migrator template code</param>
        /// <param name="state">The migrator state</param>
        /// <param name="resource">The resource state</param>
        /// <param name="position">The migrator position</param>
        /// <returns>The <see cref="ResourceMigrationState"/></returns>
        public static ResourceMigrationState CreateFrom(
            string templateCode,
            MigratorConfigurationState state,
            ResourceConfigurationState resource,
            EnMigratorPosition position)
        {
            var sourcePoint = position == EnMigratorPosition.New ? null : state.LastDefinedPoint;
            var destinationPoint = position == EnMigratorPosition.Obsolete ? null : state.LastDefinedPoint;

            var migrationToSourceExecutor = resource.CurrentPoint == state.LastDefinedPoint
                                                ? null
                                                : position == EnMigratorPosition.Obsolete
                                                  && state.MigrationPoints.Contains(resource.CurrentPoint)
                                                  && state.LastDefinedPoint != resource.CurrentPoint
                                                    ? EnMigrationSide.Source
                                                    : (EnMigrationSide?)null;

            var migrationToDestinationExecutor = resource.CurrentPoint == state.LastDefinedPoint
                                                     ? null
                                                     : position == EnMigratorPosition.New
                                                         ? EnMigrationSide.Destination
                                                         : (EnMigrationSide?)null;

            var resourceMigrationState = new ResourceMigrationState
                                             {
                                                 Name = resource.Name,
                                                 Code = resource.Code,
                                                 CurrentPoint = resource.CurrentPoint,
                                                 SourcePoint = sourcePoint,
                                                 DestinationPoint = destinationPoint,
                                                 MigrationToSourceExecutor = migrationToSourceExecutor,
                                                 MigrationToDestinationExecutor = migrationToDestinationExecutor,
                                                 MigratorTypeName = state.TypeName,
                                                 TemplateCode = templateCode
                                             };
            resourceMigrationState.SetPosition();
            return resourceMigrationState;
        }

        /// <summary>
        /// Creates <see cref="ResourceMigrationState"/>
        /// </summary>
        /// <param name="templateCode">The migrator template code</param>
        /// <param name="sourceMigratorState">
        /// The migrator state in source configuration
        /// </param>
        /// <param name="sourceState">
        /// The resource state in source configuration
        /// </param>
        /// <param name="destinationMigratorState">
        /// The migrator state in destination configuration
        /// </param>
        /// <param name="destinationState">
        /// The resource state in destination configuration
        /// </param>
        /// <returns>
        /// The <see cref="ResourceMigrationState"/>
        /// </returns>
        public static ResourceMigrationState CreateFrom(
            string templateCode,
            MigratorConfigurationState sourceMigratorState,
            ResourceConfigurationState sourceState,
            MigratorConfigurationState destinationMigratorState,
            ResourceConfigurationState destinationState)
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

            var resourceMigrationState =
                new ResourceMigrationState
                    {
                        Code = destinationState.Code,
                        Name = destinationState.Name,
                        MigratorTypeName = destinationMigratorState.TypeName,
                        TemplateCode = templateCode,
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
            resourceMigrationState.SetPosition();
            return resourceMigrationState;
        }

        /// <summary>
        /// Sets the resource current position
        /// </summary>
        private void SetPosition()
        {
            if (string.IsNullOrWhiteSpace(this.CurrentPoint))
            {
                this.Position = EnResourcePosition.NotCreated;
            }
            else if (string.IsNullOrWhiteSpace(this.DestinationPoint))
            {
                this.Position = EnResourcePosition.Obsolete;
            }
            else if (this.SourcePoint == this.CurrentPoint && this.DestinationPoint == this.CurrentPoint)
            {
                this.Position = EnResourcePosition.SourceAndDestination;
            }
            else if (this.SourcePoint == this.CurrentPoint)
            {
                this.Position = EnResourcePosition.Source;
            }
            else if (this.DestinationPoint == this.CurrentPoint)
            {
                this.Position = EnResourcePosition.Destination;
            }
            else if (this.MigrationToDestinationExecutor != null || this.MigrationToSourceExecutor != null)
            {
                this.Position = EnResourcePosition.InScope;
            }
            else
            {
                this.Position = EnResourcePosition.OutOfScope;
            }
        }
    }
}
