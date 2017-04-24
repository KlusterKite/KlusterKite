// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestDeclined.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The response from the <see cref="MigrationActor" /> that request was declined
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages.Migration
{
    using System.Collections.Generic;

    using ClusterKit.API.Client;
    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// The response from the <see cref="MigrationActor"/> that request was declined
    /// </summary>
    public class RequestDeclined
    {
        /// <summary>
        /// Gets or sets the list of errors
        /// </summary>
        public List<MigrationError> Errors { get; set; }
    }
}
