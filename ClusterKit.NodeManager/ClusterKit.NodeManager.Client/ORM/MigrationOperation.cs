// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationOperation.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the MigrationOperation type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The cluster migration operation
    /// </summary>
    [ApiDescription("The cluster migration operation", Name = "MigrationOperation")]
    public class MigrationOperation
    {
        /// <summary>
        /// Gets or sets the operation id
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField("the operation id", IsKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the migration id
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration id")]
        [ForeignKey(nameof(Migration))]
        public int ClusterMigrationId { get; set; }

        /// <summary>
        /// Gets or sets the migration
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration")]
        public Migration Migration { get; set; }

        /// <summary>
        /// Gets or sets the operation state
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation state")]
        public EnClusterMigrationOperationState State { get; set; } = EnClusterMigrationOperationState.Waiting;

        /// <summary>
        /// Gets or sets the human readable resource name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the human readable resource name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource connection string (or some identification to help connect to such resource)
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the resource connection string (or some identification to help connect to such resource)")]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the migrator fully classified class name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migrator fully classified class name")]
        public string MigratorName { get; set; }

        /// <summary>
        /// Gets or sets the migrator template code
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migrator template code")]
        public string MigratorTemplate { get; set; }

        /// <summary>
        /// Gets or sets the operation order number
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation order number")]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the operation execution start time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation execution start time")]
        public DateTimeOffset? Started { get; set; }

        /// <summary>
        /// Gets or sets the operation execution finish time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation execution finish time")]
        public DateTimeOffset? Finished { get; set; }
    }
}