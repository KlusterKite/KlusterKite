// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterTree.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Global cluster actors tree scan result
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Monitoring.Client.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// Global cluster actors tree scan result
    /// </summary>
    public class ClusterTree
    {
        /// <summary>
        /// Gets or sets the list of nodes
        /// </summary>
        public Dictionary<string, Node> Nodes { get; set; } = new Dictionary<string, Node>();

        /// <summary>
        /// Gets the total queue size
        /// </summary>
        [UsedImplicitly]
        public int QueueSizeSum
        {
            get
            {
                return (this.Nodes?.Count ?? 0) == 0 
                    ? 0 
                      // ReSharper disable once PossibleNullReferenceException
                    : this.Nodes.Values.Sum(n => n.QueueSizeSum + n.QueueSize);
            }
        }

        /// <summary>
        /// Gets the total queue size
        /// </summary>
        [UsedImplicitly]
        public int MaxQueueSize
        {
            get
            {
                return (this.Nodes?.Count ?? 0) == 0
                    ? 0
                    // ReSharper disable once PossibleNullReferenceException
                    : this.Nodes.Values.Max(n => Math.Max(n.MaxQueueSize, n.QueueSize));
            }
        }
    }
}
