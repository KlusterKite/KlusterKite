// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GuaranteeSenderActor.cs" company="TaxiKit">
//   All rights reserved
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace TaxiKit.Core.Guarantee
{
    using System;

    using Akka.Actor;
    using Akka.Event;
    using Akka.Routing;
    using Akka.Util.Internal;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    /// <summary>
    /// This actor will froward messages to cluster router actor and verify that other node will receive it
    /// </summary>
    [UsedImplicitly]
    public class GuaranteeSenderActor : UntypedActor
    {
        /// <summary>
        /// Name of child actor to distribute messages
        /// </summary>
        private const string ClusterReceiver = "receiver";

        /// <summary>
        /// Name of child actor, that contains workers for message processing
        /// </summary>
        private const string Workers = "workers";

        /// <summary>
        /// The delivery timeout.
        /// </summary>
        private readonly TimeSpan deliveryTimeout;

        /// <summary>
        /// Maximum number of attempts to deliver message to cluster
        /// </summary>
        private readonly int maxAttempts;

        /// <summary>
        /// The time period between sequential attempts to deliver same message.
        /// </summary>
        private readonly TimeSpan nextAttemptWait;

        /// <summary>
        /// Access to child actor for distributing messages
        /// </summary>
        private readonly IActorRef receiver;

        /// <summary>
        /// The redis connection.
        /// </summary>
        private readonly IConnectionMultiplexer redisConnection;

        /// <summary>
        /// Every message takes some time to process, so we will use workers to do it in parallel
        /// </summary>
        private readonly IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="GuaranteeSenderActor"/> class.
        /// </summary>
        /// <param name="redisConnection">
        /// The redis connection.
        /// </param>
        public GuaranteeSenderActor(IConnectionMultiplexer redisConnection)
        {
            var config =
                Context.System.AsInstanceOf<ExtendedActorSystem>().Provider.Deployer.Lookup(this.Self.Path).Config;

            this.maxAttempts = config.GetInt("maxAttempts", -1);
            this.deliveryTimeout = config.GetTimeSpan("deliveryTimeout", TimeSpan.FromMilliseconds(200));
            this.nextAttemptWait = config.GetTimeSpan("nextAttemptWait", TimeSpan.FromMilliseconds(500));

            this.redisConnection = redisConnection;
            this.receiver = Context.ActorOf(Props.Empty.WithRouter(FromConfig.Instance), ClusterReceiver);
            this.workers =
                Context.ActorOf(
                    Props.Create(
                        () =>
                        new Worker(
                            this.Self,
                            this.receiver,
                            this.redisConnection,
                            this.maxAttempts,
                            this.deliveryTimeout,
                            this.nextAttemptWait)).WithRouter(new ConsistentHashingPool(5)),
                    Workers);
        }

        /// <summary>
        /// Gets the delivery timeout.
        /// </summary>
        public TimeSpan DeliveryTimeout => this.deliveryTimeout;

        /// <summary>
        /// Gets the maximum number of attempts to deliver message to cluster
        /// </summary>
        public int MaxAttempts => this.maxAttempts;

        /// <summary>
        /// Gets the time period between sequential attempts to deliver same message.
        /// </summary>
        public TimeSpan NextAttemptWait => this.nextAttemptWait;

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
            /// The delivery timeout.
            /// </summary>
            private readonly TimeSpan deliveryTimeout;

            /// <summary>
            /// The main sender actor reference
            /// </summary>
            private readonly IActorRef mainSender;

            /// <summary>
            /// Maximum number of attempts to deliver message to cluster
            /// </summary>
            private readonly int maxAttempts;

            /// <summary>
            /// The time period between sequential attempts to deliver same message.
            /// </summary>
            private readonly TimeSpan nextAttemptWait;

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
            /// <param name="mainSender">
            /// The main sender actor reference
            /// </param>
            /// <param name="receiver">
            /// The receiver.
            /// </param>
            /// <param name="redisConnection">
            /// The redis connection.
            /// </param>
            /// <param name="maxAttempts">
            /// Maximum number of attempts to deliver message to cluster
            /// </param>
            /// <param name="deliveryTimeout">
            /// The delivery Timeout.
            /// </param>
            /// <param name="nextAttemptWait">
            /// The time period between sequential attempts to deliver same message.
            /// </param>
            public Worker(IActorRef mainSender, IActorRef receiver, IConnectionMultiplexer redisConnection, int maxAttempts, TimeSpan deliveryTimeout, TimeSpan nextAttemptWait)
            {
                this.mainSender = mainSender;
                this.receiver = receiver;
                this.redisConnection = redisConnection;
                this.maxAttempts = maxAttempts;
                this.deliveryTimeout = deliveryTimeout;
                this.nextAttemptWait = nextAttemptWait;

                this.Receive<GuaranteeEnvelope>(e => this.OnResendMesage(e));
                this.Receive<object>(m => this.CreateEnvelope(m));
            }

            protected override bool AroundReceive(Receive receive, object message)
            {
                Context.GetLogger().Info("recieved {messageType}", message.GetType().Name);
                return base.AroundReceive(receive, message);
            }

            /// <summary>
            /// Puts message into envelope and sends to cluster
            /// </summary>
            /// <param name="message">
            /// The original message
            /// </param>
            private void CreateEnvelope(object message)
            {
                var uid = Guid.NewGuid();
                var db = this.redisConnection.GetDatabase();
                db.StringSet(string.Format(GuaranteeEnvelope.RedisKeyFormat, uid), "1", TimeSpan.FromDays(1));

                var envelope = new GuaranteeEnvelope
                {
                    MessageId = uid,
                    Sender = this.Sender,
                    Message = message
                };

                this.OnResendMesage(envelope);
            }

            /// <summary>
            /// Forwarding message to cluster and waiting for receive confirmation
            /// </summary>
            /// <param name="envelope">
            /// Serialized message
            /// </param>
            private void OnResendMesage(GuaranteeEnvelope envelope)
            {
                try
                {
                    this.receiver.Ask<bool>(envelope, this.deliveryTimeout).Wait();
                }
                catch (Exception)
                {
                    if (this.maxAttempts <= 0 || this.maxAttempts >= envelope.CurentAttemptCount)
                    {
                        if (this.nextAttemptWait >= TimeSpan.Zero)
                        {
                            Context.System.Scheduler.ScheduleTellOnce(this.nextAttemptWait, this.mainSender, envelope.MakeNewAttempt(), ActorRefs.NoSender);
                        }
                        else
                        {
                            this.mainSender.Tell(envelope.MakeNewAttempt(), ActorRefs.NoSender);
                        }
                    }
                }
            }
        }
    }
}