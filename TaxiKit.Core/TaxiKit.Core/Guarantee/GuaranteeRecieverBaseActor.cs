// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeRecieverBaseActor.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   This actor receives gaurantee meesages, decompose them and forwards to original receiver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Guarantee
{
    using System.Threading.Tasks;

    using Akka.Actor;

    using StackExchange.Redis;

    /// <summary>
    /// This actor receives messages <seealso cref="GuaranteeSenderActor"/>, decompose them and forwards to original receiver
    /// </summary>
    public abstract class GuaranteeRecieverBaseActor : ReceiveActor
    {
        /// <summary>
        /// Connection to redis database
        /// </summary>
        private IConnectionMultiplexer redisConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeRecieverBaseActor"/> class.
        /// </summary>
        /// <param name="redisConnection">
        /// The redis connection.
        /// </param>
        protected GuaranteeRecieverBaseActor(IConnectionMultiplexer redisConnection)
        {
            this.redisConnection = redisConnection;
            this.Receive<GuaranteeEnvelope>(message => this.OnGuaranteeEnvelope(message));
        }

        /// <summary>
        /// Processing the receive of envelope from other node
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void OnGuaranteeEnvelope(GuaranteeEnvelope message)
        {
            // we should approve successfull receive of message
            this.Sender.Tell(true);
        }
    }
}