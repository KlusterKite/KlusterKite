// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Migration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The history record describing cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;
    using ClusterKit.Data.CRUD;

    using JetBrains.Annotations;

    /// <summary>
    /// The history record describing cluster migration
    /// </summary>
    [ApiDescription("The history record describing cluster migration", Name = "Migration")]
    [Serializable]
    public class Migration : IObjectWithId<int>
    {
        /// <summary>
        /// Gets or sets the migration id
        /// </summary>
        [Key]
        [UsedImplicitly]
        [DeclareField("The migration id", IsKey = true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")]  // TODO: check and remove that Npgsql.EntityFrameworkCore.PostgreSQL can generate serial columns on migration without this kludge
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether current migration is in process
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating that current migration is in process")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the migration state
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration state")]
        public EnMigrationState State { get; set; }

        /// <summary>
        /// Gets or sets the migration direction
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration direction")]
        public EnMigrationDirection? Direction { get; set; }

        /// <summary>
        /// Gets or sets the migration start time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration start time")]
        public DateTimeOffset Started { get; set; }

        /// <summary>
        /// Gets or sets the migration finish time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the migration finish time")]
        public DateTimeOffset? Finished { get; set; }

        /// <summary>
        /// Gets or sets the list of migration logs
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of migration logs")]
        public List<MigrationLogRecord> Logs { get; set; }

        /// <summary>
        /// Gets or sets the previous release id
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(FromRelease))]
        [DeclareField("the previous release id")]
        public int FromReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the new release id
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(ToRelease))]
        [DeclareField("the new release id")]
        public int ToReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the previous release
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the previous release")]
        public Release FromRelease { get; set; }

        /// <summary>
        /// Gets or sets the new release
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the new release")]
        public Release ToRelease { get; set; }

        /// <inheritdoc />
        public int GetId()
        {
            return this.Id;
        }
    }
}
