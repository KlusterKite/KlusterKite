// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnMigrationDirection.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The direction of cluster migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.ORM
{
    using KlusterKite.API.Attributes;

    /// <summary>
    /// The direction of cluster migration
    /// </summary>
    [ApiDescription("The direction of cluster migration", Name = "EnMigrationDirection")]
    public enum EnMigrationDirection
    {
        /// <summary>
        /// The cluster is upgrading
        /// </summary>
        [ApiDescription("The cluster is upgrading")]
        Upgrade,

        /// <summary>
        /// The cluster is downgrading
        /// </summary>
        [ApiDescription("The cluster is downgrading")]
        Downgrade,

        /// <summary>
        /// The cluster resources don't need upgrade. They are up to date.
        /// </summary>
        [ApiDescription("The cluster resources don't need upgrade. They are up to date.")]
        Stay,

        /// <summary>
        /// The migration direction can-not be evaluated. The migration is corrupted.
        /// </summary>
        [ApiDescription("The migration direction can-not be evaluated. The migration is corrupted.")]
        Undefined
    }
}