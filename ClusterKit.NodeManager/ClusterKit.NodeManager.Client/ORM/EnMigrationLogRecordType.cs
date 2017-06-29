// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationLogRecordType.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The types of migration log records
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;

    using ClusterKit.API.Attributes;

    /// <summary>
    /// The types of migration log records
    /// </summary>
    [ApiDescription(Name = "EnMigrationLogRecordType")]
    [Flags]
    public enum EnMigrationLogRecordType
    {
        /// <summary>
        /// The successful operation
        /// </summary>
        [ApiDescription("The successful operation")]
        Operation = 1,

        /// <summary>
        /// Some error
        /// </summary>
        [ApiDescription("Some error")]
        Error = 2,

        /// <summary>
        /// The faulted operation
        /// </summary>
        [ApiDescription("The faulted operation")]
        OperationError = 3
    }
}