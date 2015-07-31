// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AkkaClusterTest.cs" company="">
//
// </copyright>
// <summary>
//   Akka cluster capability testing
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TaxiKit.Core.Tests.ConceptProof
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Threading;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;
    using Akka.Event;

    using Serilog;

    using TaxiKit.Core.Cluster;
    using TaxiKit.Core.TestKit;

    using Xunit;
    using Xunit.Abstractions;

    using static Serilog.Log;

    /// <summary>
    /// Akka cluster capability testing
    /// </summary>
    public class AkkaClusterTest : TestWithSerilog
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AkkaClusterTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The Xunit output.
        /// </param>
        public AkkaClusterTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// Experimenting with akka cluster initialization
        /// </summary>
        [Fact]
        public void SimpleClusterTest()
        {
            var ports = new[] { "2551", "2552", "0", "0" };

            var baseConfig = ConfigurationFactory.Empty
                .WithFallback(ConfigurationFactory.ParseString("akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                .WithFallback(ConfigurationFactory.ParseString("akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                .WithFallback(ConfigurationFactory.ParseString("akka.cluster.seed-nodes = [\"akka.tcp://ClusterSystem@127.0.0.1:2551\", \"akka.tcp://ClusterSystem@127.0.0.1:2552\"]"))
                .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [\"xunit\"]"))
                .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 30s"));

            var systems = new List<ActorSystem>();
            var systemUpWaitHandles = new List<EventWaitHandle>();

            DateTimeOffset now = DateTimeOffset.Now;

            foreach (var port in ports)
            {
                // Override the configuration of the port
                var config = ConfigurationFactory.Empty
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.helios.tcp.hostname = 127.0.0.1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.helios.tcp.port=" + port))
                    .WithFallback(baseConfig);

                // create an Akka system
                ActorSystem system = ActorSystem.Create("ClusterSystem", config);
                systems.Add(system);
                Logger.Information("node {0} is up", port);
                var waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                systemUpWaitHandles.Add(waitHandle);
                system.ActorOf(Props.Create(() => new ClusterWaitActor(4, waitHandle)), "ClusterWait");
                Thread.Sleep(TimeSpan.FromSeconds(2));
            }

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            Log.Information("Cluster is UP and waiting ({Ms} for start)", (DateTimeOffset.Now - now).TotalMilliseconds);

            foreach (var system in systems)
            {
                system.Shutdown();
            }
        }

        /// <summary>
        /// Actor for cluster build lookup
        /// </summary>
        private class ClusterWaitActor : ReceiveActor
        {
            /// <summary>
            /// The cluster.
            /// </summary>
            private Akka.Cluster.Cluster cluster;

            /// <summary>
            /// Test cluster expected nodes count
            /// </summary>
            private int expectedNodesCount;

            /// <summary>
            /// Current known members (And I always know myself)
            /// </summary>
            private int membersCount = 1;

            private readonly EventWaitHandle nodesUp;

            public ClusterWaitActor(int expectedNodesCount, EventWaitHandle nodesUp)
            {
                this.expectedNodesCount = expectedNodesCount;
                this.nodesUp = nodesUp;

                this.Receive<ClusterEvent.MemberUp>(m => this.OnMemberUp());
            }

            /// <summary>
            /// Need to subscribe to cluster changes
            /// </summary>
            protected override void PreStart()
            {
                this.cluster = Cluster.Get(Context.System);
                this.cluster.Subscribe(
                    this.Self,
                    ClusterEvent.InitialStateAsEvents,
                    new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.IReachabilityEvent) });
            }

            private void OnMemberUp()
            {
                this.membersCount++;
                Context.GetLogger().Info(
                    "{Address}: {membersCount} / {expectedNodesCount} is up",
                    this.cluster.SelfAddress.ToString(),
                    this.membersCount,
                    this.expectedNodesCount);
                if (this.membersCount == this.expectedNodesCount)
                {
                    this.nodesUp.Set();
                }
            }

            /// <summary>
            /// Re-subscribe on restart
            /// </summary>
            protected override void PostStop()
            {
                this.cluster.Unsubscribe(this.Self);
            }
        }
    }
}