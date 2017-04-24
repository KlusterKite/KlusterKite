// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorInitializationFailed.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The notification of current migration check complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages.Migration
{
    using System.Collections.Generic;

    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// The notification of resource migration state check failed
    /// </summary>
    public class MigrationActorInitializationFailed
    {
        /// <summary>
        /// Gets or sets the initialization errors
        /// </summary>
        public List<MigrationError> Errors { get; set; }
    }
}
