// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClusterTree.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Global cluster actors tree scan result
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Monitoring.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Client.Converters;
    using KlusterKite.Monitoring.Client.Messages;

    /// <summary>
    /// Global cluster actors tree scan result
    /// </summary>
    [ApiDescription("Global cluster actors tree scan result", Name = "ClusterTree")]
    public class ClusterTree
    {
        /// <summary>
        /// Gets the maximum queue size among tree nodes
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Gets the maximum queue size among tree nodes")]
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

        /// <summary>
        /// Gets the list of nodes
        /// </summary>
        [DeclareField("Gets the list of nodes", Converter = typeof(DictionaryConverter<string, Node>))]
        public Dictionary<string, Node> Nodes { get; } = new Dictionary<string, Node>();

        /// <summary>
        /// Gets the nodes flat list
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Gets the  nodes flat list")]
        public IEnumerable<Node> NodesFlat => this.FlattenTree(this.Nodes.Values);

        /// <summary>
        /// Gets the sum of queue size across tree
        /// </summary>
        [UsedImplicitly]
        [DeclareField("Gets the sum of queue size across tree")]
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
        /// Flattens tree of nodes
        /// </summary>
        /// <param name="nodesValues">The list of nodes</param>
        /// <returns>The flattened tree of nodes</returns>
        private IEnumerable<Node> FlattenTree(IEnumerable<Node> nodesValues)
        {
            foreach (var node in nodesValues)
            {
                yield return node;
                if (node.Children != null)
                {
                    foreach (var child in this.FlattenTree(node.Children))
                    {
                        yield return child;
                    }
                }
            }
        }
    }
}