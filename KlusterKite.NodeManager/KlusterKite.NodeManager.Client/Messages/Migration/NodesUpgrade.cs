// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodesUpgrade.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The command to the node manager to upgrade cluster node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using KlusterKite.NodeManager.Client.MigrationStates;

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
