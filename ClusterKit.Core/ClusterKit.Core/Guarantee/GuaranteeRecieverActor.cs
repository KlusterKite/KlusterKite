// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeRecieverActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   This actor receives gaurantee meesages, decompose them and forwards to original receiver
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Guarantee
{
    using Akka.Actor;
    using Akka.Event;
    using Akka.Util.Internal;

    using StackExchange.Redis;

    /// <summary>
    /// This actor receives messages <seealso cref="GuaranteeSenderActor"/>, decompose them and forwards to original receiver
    /// </summary>
    public class GuaranteeRecieverActor : ReceiveActor
    {
        /// <summary>
        /// Local path to the final message receiver
        /// </summary>
        private readonly string endReciever;

        /// <summary>
        /// Connection to redis database
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeRecieverActor"/> class.
        /// </summary>
        /// <param name="redisConnection">
        /// The redis connection.
        /// </param>
        public GuaranteeRecieverActor(IConnectionMultiplexer redisConnection)
        {
            this.redisConnection = redisConnection;
            var config =
                Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path).Config;

            this.endReciever = config.GetString("endReciever");
            if (string.IsNullOrEmpty(this.endReciever))
            {
                Context.GetLogger().Error(
                    "{Type}: {Path} has no configured receiver",
                    this.GetType().Name,
                    this.Self.Path.ToString());
                this.Self.Tell(PoisonPill.Instance);
            }

            this.Receive<GuaranteeEnvelope>(message => this.OnGuaranteeEnvelope(message));
        }

        /// <summary>
        /// Gets local path to the final message receiver
        /// </summary>
        public string EndReciever => this.endReciever;

        /// <summary>
        /// Processing the receive of envelope from other node
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        private void OnGuaranteeEnvelope(GuaranteeEnvelope message)
        {
            var redis = this.redisConnection.GetDatabase();
            var isFirst = redis.StringGetSet(
                string.Format(GuaranteeEnvelope.RedisKeyFormat, message.MessageId),
                string.Empty);

            if (!string.IsNullOrEmpty(isFirst))
            {
                Context.ActorSelection(this.endReciever).Tell(message.Message, message.Sender);
                this.Sender.Tell(true);
            }
            else
            {
                this.Sender.Tell(false);
            }
        }
    }
}