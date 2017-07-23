// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Node.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The actor's description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Monitoring.Client.Messages
{
    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;

    /// <summary>
    /// The actor's description
    /// </summary>
    [ApiDescription("The actor's description", Name = "Node")]
    public class Node
    {
        /// <summary>
        /// Gets or sets the actor's type
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the actor's type")]
        public string ActorType { get; set; }

        /// <summary>
        /// Gets or sets the actor's last message
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the actor's last message")]
        public string CurrentMessage { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher id
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the actor's dispatcher id")]
        public string DispatcherId { get; set; }

        /// <summary>
        /// Gets or sets the actor's dispatcher type
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the actor's dispatcher type")]
        public string DispatcherType { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        [DeclareField("the maximum queue size among it's children")]
        public int MaxQueueSize { get; set; }

        /// <summary>
        /// Gets or sets the actor's name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the actor's name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the actor's queue size
        /// </summary>
        [DeclareField("the actor's queue size")]
        public int QueueSize { get; set; }

        /// <summary>
        /// Gets or sets the maximum queue size among it's children
        /// </summary>
        [DeclareField("the maximum queue size among it's children")]
        public int QueueSizeSum { get; set; }

        /// <summary>
        /// Gets or sets the actor's children
        /// </summary>
        [DeclareField("the actor's children")]
        public Node[] Children { get; set; }

        /// <summary>
        /// Gets or sets the actor's address
        /// </summary>
        [DeclareField("the actor's address", IsKey = true)]
        [UsedImplicitly]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets the actor's parent's address
        /// </summary>
        [DeclareField("the actor's parent's address")]
        [UsedImplicitly]
        public string ParentAddress { get; set; }
    }
}
