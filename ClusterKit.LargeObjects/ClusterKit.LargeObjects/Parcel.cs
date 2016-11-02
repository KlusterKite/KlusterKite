// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Parcel.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates message with large payload
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.LargeObjects
{
    using System;

    using Akka.Actor;
    using Akka.Routing;

    using ClusterKit.Core;

    /// <summary>
    /// Creates message with large payload
    /// </summary>
    public class Parcel : IShardedMessage, IConsistentHashable
    {
        /// <summary>
        /// Gets or sets the message payload
        /// </summary>
        public object Payload { get; set; }

        /// <summary>
        /// Gets or sets the original message receiver
        /// <remarks>Multiple receivers are not supported</remarks>
        /// </summary>
        public ICanTell Recipient { get; set; }

        /// <summary>
        /// Gets or sets the original message sender
        /// </summary>
        public IActorRef Sender { get; set; }

        /// <summary>
        /// Gets or sets parcel storage timeout.
        /// </summary>
        public TimeSpan StoreTimeout { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Gets or sets parcel sent time.
        /// </summary>
        public DateTimeOffset SentTime { get; set; } = DateTimeOffset.Now;

        /// <summary>
        /// Gets the parcel unique identification number
        /// </summary>
        public Guid Uid { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the entity id of the end-point receiver, if applicable
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets the shard id of the end-point receiver, if applicable
        /// </summary>
        public string ShardId { get; set; }

        /// <summary>
        /// Gets or sets the consistent hash key  of the end-point receiver, if applicable
        /// </summary>
        public object ConsistentHashKey { get; set; }
    }
}
