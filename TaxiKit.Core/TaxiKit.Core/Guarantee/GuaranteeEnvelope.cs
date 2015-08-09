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

    /// <summary>
    /// Composed message for sending via <seealso cref="GuaranteeSenderActor"/> - <seealso cref="GuaranteeRecieverActor"/> network
    /// </summary>
    public class GuaranteeEnvelope
    {
        /// <summary>
        /// Key format for storing serialized messages in redis
        /// </summary>
        internal const string RedisKeyForma = "Network:GuaranteeMessage:{0}";

        /// <summary>
        /// Gets or sets number of attempts made to deliver message to cluster node
        /// </summary>
        public int CuurentAttemptCount { get; private set; } = 1;

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
                CuurentAttemptCount = this.CuurentAttemptCount + 1
            };
        }
    }
}