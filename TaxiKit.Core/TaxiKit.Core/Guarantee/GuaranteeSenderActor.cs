// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeSenderActor.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// <summary>
//   This actor will froward messages to cluster router actor and verify that other node will recieve it
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Guarantee
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Routing;

    using StackExchange.Redis;

    /// <summary>
    /// This actor will froward messages to cluster router actor and verify that other node will receive it
    /// </summary>
    public class GuaranteeSenderActor : UntypedActor
    {
        /// <summary>
        /// Name of child actor to distribute messages
        /// </summary>
        public const string ClusterReceiver = "receiver";

        /// <summary>
        /// Name of child actor, that contains workers for message processing
        /// </summary>
        public const string Workers = "workers";

        /// <summary>
        /// The redis connection.
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        /// <summary>
        /// Access to child actor for distributing messages
        /// </summary>
        private IActorRef receiver;

        /// <summary>
        /// Every message takes some time to process, so we will use workers to do it in parallel
        /// </summary>
        private IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeSenderActor"/> class.
        /// </summary>
        /// <param name="redisConnection">
        /// The redis connection.
        /// </param>
        public GuaranteeSenderActor(IConnectionMultiplexer redisConnection)
        {
            this.redisConnection = redisConnection;
            this.receiver = Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), ClusterReceiver);
            this.workers = Context.ActorOf(Props.Create(() => new Worker(this.receiver, this.redisConnection)).WithRouter(FromConfig.Instance), Workers);
        }

        /// <summary>
        /// Forwarding all incoming messages to worker actors
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        protected override void OnReceive(object message)
        {
            this.workers.Forward(message);
        }

        /// <summary>
        /// Workers, that should serialize message and send them to other cluster nodes
        /// </summary>
        private class Worker : ReceiveActor
        {
            /// <summary>
            /// Access to child actor for distributing messages across actor
            /// </summary>
            private readonly IActorRef receiver;

            /// <summary>
            /// The redis connection.
            /// </summary>
            private readonly IConnectionMultiplexer redisConnection;

            /// <summary>
            /// Initializes a new instance of the <see cref="Worker"/> class.
            /// </summary>
            /// <param name="receiver">
            /// The receiver.
            /// </param>
            /// <param name="redisConnection">
            /// The redis connection.
            /// </param>
            public Worker(IActorRef receiver, IConnectionMultiplexer redisConnection)
            {
                this.receiver = receiver;
                this.redisConnection = redisConnection;

                this.Receive<GuaranteeEnvelope>(e => this.OnResendMesage(e));
                this.Receive<object>(m => this.CreateEnvelope(m));
            }

            /// <summary>
            /// Puts message into envelope and sends to cluster
            /// </summary>
            /// <param name="message">The original message</param>
            private void CreateEnvelope(object message)
            {
                // todo: @kantora create and send envelopes
                var envelope = new GuaranteeEnvelope();
                this.OnResendMesage(envelope);
            }

            /// <summary>
            /// Forwarding message to cluster and waiting for recieve confirmation
            /// </summary>
            /// <param name="envelope">Serialized message</param>
            private void OnResendMesage(GuaranteeEnvelope envelope)
            {
            }
        }
    }
}