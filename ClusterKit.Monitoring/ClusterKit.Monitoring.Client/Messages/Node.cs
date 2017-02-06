// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Node.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The actor's description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Monitoring.Client.Messages
{
    using System.Xml.Serialization;

    using JetBrains.Annotations;

    /// <summary>
    /// The actor's description
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Gets or sets the actor's type
        /// </summary>
        [UsedImplicitly]
        public string ActorType { get; set; }

        /// <summary>
        /// Gets or sets the actor's last message
        /// </summary>
        [UsedImplicitly]
        public string CurrentMessage { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher id
        /// </summary>
        [UsedImplicitly]
        public string DispatcherId { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher type
        /// </summary>
        [UsedImplicitly]
        public string DispatcherType { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the actor's name
        /// </summary>
        [UsedImplicitly]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actor's queue size
        /// </summary>
        public int QueueSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        public int QueueSizeSum { get; set; }

        /// <summary>
        /// Gets or sets the actor's children
        /// </summary>
        [XmlArray("children")]
        public Node[] Children { get; set; }
    }
}
