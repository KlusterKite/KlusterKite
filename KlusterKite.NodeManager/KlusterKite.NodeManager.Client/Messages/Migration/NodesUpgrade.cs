// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodesUpgrade.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The command to the node manager to upgrade cluster node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using ClusterKit.NodeManager.Client.MigrationStates;

    /// <summary>
    /// The command to the node manager to upgrade cluster node
    /// </summary>
    public class NodesUpgrade
    {
        /// <summary>
        /// Gets or sets a target migration point
        /// </summary>
        public EnMigrationSide Target { get; set; }
    }
}
