// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeUpgradeRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Manual request to upgrade spceific node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using Akka.Actor;

    /// <summary>
    /// Manual request to upgrade specific node
    /// </summary>
    public class NodeUpgradeRequest
    {
        /// <summary>
        /// Gets or sets address of node to upgrade
        /// </summary>
        public Address Address { get; set; }
    }
}