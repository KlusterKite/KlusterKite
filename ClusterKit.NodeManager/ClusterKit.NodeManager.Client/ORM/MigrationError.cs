// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationError.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The error description during the resource check and/or migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The error description during the resource check and/or migration
    /// </summary>
    [ApiDescription("The error description during the resource check and/or migration", Name = "MigrationError")]
    [Table("MigrationErrors")]
    public class MigrationError : MigrationLogRecord
    {
        /// <summary>
        /// Gets or sets the created date
        /// </summary>
        [DeclareField("the error occurrence time")]
        [UsedImplicitly]
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        [DeclareField("the resource name")]
        [UsedImplicitly]
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error stack trace
        /// </summary>
        [DeclareField("the error stack trace")]
        [UsedImplicitly]
        public string ErrorStackTrace { get; set; }
    }
}
