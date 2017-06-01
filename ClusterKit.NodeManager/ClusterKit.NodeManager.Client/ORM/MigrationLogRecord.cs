// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationLogRecord.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The base class for migration log records
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
    /// The base class for migration log records
    /// </summary>
    [Table("MigrationLogRecords")]
    [Serializable]
    public abstract class MigrationLogRecord
    {
        /// <summary>
        /// Gets or sets the error id
        /// </summary>
        [Key]
        [DeclareField("the log record id", IsKey = true)]
        [UsedImplicitly]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "serial")]  // TODO: check and remove that Npgsql.EntityFrameworkCore.PostgreSQL can generate serial columns on migration without this kludge
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the migration id
        /// </summary>
        [DeclareField("the migration id")]
        [UsedImplicitly]
        public int? MigrationId { get; set; }

        /// <summary>
        /// Gets or sets the migration
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(MigrationId))]
        public Migration Migration { get; set; }

        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        [ForeignKey(nameof(Release))]
        [DeclareField("the release id")]
        [UsedImplicitly]
        public int ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the release
        /// </summary>
        [DeclareField("the release")]
        [UsedImplicitly]
        public Release Release { get; set; }

        /// <summary>
        /// Gets or sets the migrator template code
        /// </summary>
        [DeclareField("the migrator template code")]
        [UsedImplicitly]
        public string MigratorTemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the migrator template name
        /// </summary>
        [DeclareField("the migrator template name")]
        [UsedImplicitly]
        public string MigratorTemplateName { get; set; }

        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        [DeclareField("the migrator type name")]
        [UsedImplicitly]
        public string MigratorTypeName { get; set; }

        /// <summary>
        /// Gets or sets the migrator name
        /// </summary>
        [DeclareField("the migrator name")]
        [UsedImplicitly]
        public string MigratorName { get; set; }

        /// <summary>
        /// Gets or sets the resource code
        /// </summary>
        [DeclareField("the resource code")]
        [UsedImplicitly]
        public string ResourceCode { get; set; }

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        [DeclareField("the resource name")]
        [UsedImplicitly]
        public string ResourceName { get; set; }
    }
}
