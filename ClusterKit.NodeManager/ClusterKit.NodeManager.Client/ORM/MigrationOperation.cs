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
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The cluster migration operation
    /// </summary>
    [ApiDescription("The cluster migration operation", Name = "MigrationOperation")]
    [Table("MigrationOperations")]
#if APPDOMAIN
    [Serializable]
#endif
    public class MigrationOperation : MigrationLogRecord
    {
        /// <summary>
        /// Gets or sets the operation execution start time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation execution start time")]
        public DateTimeOffset Started { get; set; }

        /// <summary>
        /// Gets or sets the operation execution finish time
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the operation execution finish time")]
        public DateTimeOffset Finished { get; set; }

        /// <summary>
        /// Gets or sets the source migration point.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the source migration point")]
        public string SourcePoint { get; set; }

        /// <summary>
        /// Gets or sets the source migration point.
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the destination migration point")]
        public string DestinationPoint { get; set; }

        /// <summary>
        /// Gets or sets the execution error id
        /// </summary>
        [UsedImplicitly]
        [ForeignKey(nameof(Error))]
        [DeclareField("the execution error id")]
        public int? ErrorId { get; set; }

        /// <summary>
        /// Gets or sets the execution error
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the execution error")]
        public MigrationError Error { get; set; }
    }
}