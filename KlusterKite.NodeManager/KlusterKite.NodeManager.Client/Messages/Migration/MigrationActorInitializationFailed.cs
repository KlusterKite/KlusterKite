// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorInitializationFailed.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The notification of current migration check complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using KlusterKite.NodeManager.Client.ORM;

    /// <summary>
    /// The notification of resource migration state check failed
    /// </summary>
    public class MigrationActorInitializationFailed
    {
        /// <summary>
        /// Gets or sets the initialization errors
        /// </summary>
        public List<MigrationLogRecord> Errors { get; set; }
    }
}
