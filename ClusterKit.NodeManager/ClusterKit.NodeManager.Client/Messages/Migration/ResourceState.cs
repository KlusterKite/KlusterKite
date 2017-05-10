// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The overall resource migration state
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The overall resource migration state
    /// </summary>
    [ApiDescription("The overall resource migration state", Name = "ResourceState")]
    public class ResourceState
    {
        /// <summary>
        /// Gets or sets the current resource state with active migration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the current resource state with active migration")]
        public MigrationActorMigrationState MigrationState { get; set; }

        /// <summary>
        /// Gets or sets the current resource state without active migration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the current resource state without active migration")]
        public MigrationActorReleaseState ReleaseState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether some resource operation is in progress
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether some resource operation is in progress")]
        public bool OperationIsInProgress { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a new migration can be initiated
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether a new migration can be initiated")]
        public bool CanCreateMigration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether resource can be migrated
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether resource can be migrated")]
        public bool CanMigrateResources { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether node update can be initiated. The nodes will be updated to the destination release.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether node update can be initiated. The nodes will be updated to the destination release.")]
        public bool CanUpdateNodesToDestination { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether node update can be initiated. The nodes will be updated to the source release.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether node update can be initiated. The nodes will be updated to the source release.")]
        public bool CanUpdateNodesToSource { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration can be canceled at this stage
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether current migration can be canceled at this stage")]
        public bool CanCancelMigration { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration can be finished at this stage
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether current migration can be finished at this stage")]
        public bool CanFinishMigration { get; set; }
    }
}
