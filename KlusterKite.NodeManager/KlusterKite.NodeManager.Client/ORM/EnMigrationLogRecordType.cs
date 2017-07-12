// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationLogRecordType.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The types of migration log records
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The types of migration log records
    /// </summary>
    [ApiDescription(Name = "EnMigrationLogRecordType", Description = "The types of migration log records")]
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
        OperationError = 3,

        /// <summary>
        /// The new migration from this configuration was started
        /// </summary>
        [ApiDescription("The new migration from this configuration was started")]
        StartedFromConfiguration = 4,

        /// <summary>
        /// The migration from this configuration was finished
        /// </summary>
        [ApiDescription("The migration from this configuration was finished")]
        FinishedFromConfiguration = 5,

        /// <summary>
        /// The migration from this configuration was canceled
        /// </summary>
        [ApiDescription("The migration from this configuration was canceled")]
        CanceledFromConfiguration = 6,

        /// <summary>
        /// The new migration to this configuration was started
        /// </summary>
        [ApiDescription("The new migration to this configuration was started")]
        StartedToConfiguration = 7,

        /// <summary>
        /// The migration to this configuration was finished
        /// </summary>
        [ApiDescription("The migration to this configuration was finished")]
        FinishedToConfiguration = 8,

        /// <summary>
        /// The migration to this configuration was canceled
        /// </summary>
        [ApiDescription("The migration to this configuration was canceled")]
        CanceledToConfiguration = 9
    }
}