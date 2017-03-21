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

    using ClusterKit.API.Client.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// The actor's description
    /// </summary>
    [ApiDescription(Description = "The actor's description", Name = "ClusterKitMonitoringNode")]
    public class Node
    {
        /// <summary>
        /// Gets or sets the actor's type
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the actor's type")]
        public string ActorType { get; set; }

        /// <summary>
        /// Gets or sets the actor's last message
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the actor's last message")]
        public string CurrentMessage { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher id
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the actor's dispatcher id")]
        public string DispatcherId { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher type
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the actor's dispatcher type")]
        public string DispatcherType { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        [DeclareField(Description = "the maximum queue size among it's children")]
        public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the actor's name
        /// </summary>
        [UsedImplicitly]
        [DeclareField(Description = "the actor's name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actor's queue size
        /// </summary>
        [DeclareField(Description = "the actor's queue size")]
        public int QueueSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        [DeclareField(Description = "the maximum queue size among it's children")]
        public int QueueSizeSum { get; set; }

        /// <summary>
        /// Gets or sets the actor's children
        /// </summary>
        [XmlArray("children")]
        [DeclareField(Description = "the actor's children")]
        public Node[] Children { get; set; }
    }
}
