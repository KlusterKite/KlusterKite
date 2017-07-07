// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestDeclined.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The response from the <see cref="MigrationActor" /> that request was declined
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using KlusterKite.NodeManager.Client.ORM;

    /// <summary>
    /// The response from the MigrationActor that request was declined
    /// </summary>
    public class RequestDeclined
    {
        /// <summary>
        /// Gets or sets the list of errors
        /// </summary>
        public List<MigrationLogRecord> Errors { get; set; }
    }
}
