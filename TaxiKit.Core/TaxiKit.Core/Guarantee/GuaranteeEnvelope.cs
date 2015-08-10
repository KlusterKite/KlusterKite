// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeEnvelope.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   Composed message for sending via <seealso cref="GuaranteeSenderActor" /> - <seealso cref="GuaranteeRecieverActor" /> network
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Guarantee
{
    using System;

    using Akka.Actor;
    using Akka.Routing;

    /// <summary>
    /// Composed message for sending via <seealso cref="GuaranteeSenderActor"/> - <seealso cref="GuaranteeRecieverActor"/> network
    /// </summary>
    public class GuaranteeEnvelope : IConsistentHashable
    {
        /// <summary>
        /// Key format for storing serialized messages in redis
        /// </summary>
        internal const string RedisKeyFormat = "Network:GuaranteeMessage:{0}";

        /// <summary>
        /// Key for correct distribution among <seealso cref="ConsistentHashingGroup"/>
        /// </summary>
        public object ConsistentHashKey
        {
            get
            {
                var che = this.Message as ConsistentHashableEnvelope;
                if (che != null)
                {
                    return che.ConsistentHashKey;
                }

                var ch = this.Message as IConsistentHashable;
                if (ch != null)
                {
                    return ch.ConsistentHashKey;
                }

                return this.MessageId;
            }
        }

        /// <summary>
        /// Gets or sets number of attempts made to deliver message to cluster node
        /// </summary>
        public int CuurentAttemptCount { get; private set; } = 1;

        /// <summary>
        /// Original message object
        /// </summary>
        public object Message { get; set; }

        /// <summary>
        /// Gets or sets the message id for obtaining serialized message from redis.
        /// </summary>
        public Guid MessageId { get; set; }

        /// <summary>
        /// Gets or sets the original sender.
        /// </summary>
        public IActorRef Sender { get; set; }

        /// <summary>
        /// Creates message clone with <seealso cref="CuurentAttemptCount"/> incremented by 1
        /// </summary>
        /// <returns>The clone of current message</returns>
        public GuaranteeEnvelope MakeNewAttempt()
        {
            return new GuaranteeEnvelope
            {
                MessageId = this.MessageId,
                Sender = this.Sender,
                Message = this.Message,
                CuurentAttemptCount = this.CuurentAttemptCount + 1
            };
        }
    }
}