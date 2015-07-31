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
    using System.Collections.Immutable;
    using System.Configuration;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Cluster.Routing;
    using Akka.Configuration;
    using Akka.Configuration.Hocon;
    using Akka.Event;
    using Akka.Routing;

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
        public AkkaClusterTest(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        /// Experimenting with akka cluster initialization
        /// </summary>
        [Fact]
        public void SimpleClusterTest()
        {
            var baseConfig =
                ConfigurationFactory.Empty.WithFallback(
                    ConfigurationFactory.ParseString(
                        "akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.roles = [\"xunit\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 30s"));

            var systems = new List<ActorSystem>();
            var systemUpWaitHandles = new List<EventWaitHandle>();

            DateTimeOffset now = DateTimeOffset.Now;

            StartSystem(
                2551,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });
            StartSystem(
                2552,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });
            StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });
            StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

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
        /// Experimenting with akka cluster initialization
        /// </summary>
        [Fact]
        public void MessageRoutingTest()
        {
            var baseConfig =
                ConfigurationFactory.Empty.WithFallback(
                    ConfigurationFactory.ParseString(
                        "akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 30s"));

            var systems = new List<ActorSystem>();
            var systemUpWaitHandles = new List<EventWaitHandle>();

            DateTimeOffset now = DateTimeOffset.Now;

            var sys1 = StartSystem(
                2551,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });

            //sys1.ActorOf(Props.Create(() => new LogActor()), "log");

            var sys2 = StartSystem(
                2552,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            //sys2.ActorOf(Props.Create(() => new LogActor()), "log");

            var sys3 = StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys3.ActorOf(Props.Create(() => new LogActor()), "log");

            var sys4 = StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys4.ActorOf(Props.Create(() => new LogActor()), "log");

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            Log.Information("Cluster is UP and waiting ({Ms} for start)", (DateTimeOffset.Now - now).TotalMilliseconds);

            sys1.ActorSelection("/user/log").Tell("Hello World");
            sys2.ActorSelection("/user/log").Tell("Hello World");
            sys3.ActorSelection("/user/log").Tell("Hello World");
            sys4.ActorSelection("/user/log").Tell("Hello World");

            sys3.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2551/user/log").Tell("Hello World remote");
            sys4.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2551/user/log").Tell("Hello World remote");

            var sys2Router = sys2.ActorOf(
                    Props.Empty.WithRouter(new ClusterRouterGroup(new ConsistentHashingGroup("/user/log"),
                        new ClusterRouterGroupSettings(4, false, "role2", ImmutableHashSet.Create("/user/log")))), "logRouter");

            Log.Information(sys2Router.Path.ToString());

            for (int i = 0; i < 30; i++)
            {
                sys2Router.Tell(new ConsistentHashableEnvelope($"Hello {i}", i));
            }

            Thread.Sleep(TimeSpan.FromSeconds(1));

            foreach (var system in systems)
            {
                system.Shutdown();
            }
        }

        /// <summary>
        /// Actor test system start
        /// </summary>
        /// <param name="port">Network port</param>
        /// <param name="baseConfig">Base configuration description</param>
        /// <param name="systems">Complet cluster node list (will add new system to it)</param>
        /// <param name="systemUpWaitHandles">List of wait handles for clusters initialization waiting</param>
        /// <param name="roles">Node roles list</param>
        /// <param name="seedNodes">Seed nodes addresses list</param>
        private static ActorSystem StartSystem(
            int port,
            Config baseConfig,
            List<ActorSystem> systems,
            List<EventWaitHandle> systemUpWaitHandles,
            IEnumerable<string> roles,
            IEnumerable<string> seedNodes)
        {
            if (roles == null) roles = new string[0];
            if (seedNodes == null) seedNodes = new string[0];

            // Override the configuration of the port
            var config =
                ConfigurationFactory.Empty.WithFallback(
                    ConfigurationFactory.ParseString("akka.remote.helios.tcp.hostname = 127.0.0.1"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.remote.helios.tcp.port=" + port))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            $"akka.cluster.roles = [{string.Join(", ", roles.Select(s => $"\"{s}\""))}]"))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            $"akka.cluster.seed-nodes = [{string.Join(", ", seedNodes.Select(s => $"\"{s}\""))}]"))
                    .WithFallback(baseConfig);

            // create an Akka system
            ActorSystem system = ActorSystem.Create("ClusterSystem", config);
            systems.Add(system);
            Logger.Information("node {0} is up", port);
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            systemUpWaitHandles.Add(waitHandle);
            system.ActorOf(Props.Create(() => new ClusterWaitActor(4, waitHandle)), "ClusterWait");
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            return system;
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
            private int membersCount = 0;

            private readonly EventWaitHandle nodesUp;

            public ClusterWaitActor(int expectedNodesCount, EventWaitHandle nodesUp)
            {
                this.expectedNodesCount = expectedNodesCount;
                this.nodesUp = nodesUp;

                this.Receive<ClusterEvent.MemberUp>(m => this.OnMemberUp());
                this.Receive<ClusterEvent.UnreachableMember>(m => this.OnMemberDown());
                this.Receive<ClusterEvent.MemberRemoved>(m => this.OnMemberDown());
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
                    new[] { typeof(ClusterEvent.IMemberEvent), typeof(ClusterEvent.UnreachableMember) });
            }

            private void OnMemberDown()
            {
                this.membersCount--;
                Context.GetLogger()
                    .Info(
                        "{Address}: {membersCount} / {expectedNodesCount} is down",
                        this.cluster.SelfAddress.Port.ToString(),
                        this.membersCount,
                        this.expectedNodesCount);
            }

            private void OnMemberUp()
            {
                this.membersCount++;
                Context.GetLogger()
                    .Info(
                        "{Address}: {membersCount} / {expectedNodesCount} is up",
                        this.cluster.SelfAddress.Port.ToString(),
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

        /// <summary>
        /// Simple actor to log strings, recieved as messaages
        /// </summary>
        private class LogActor : ReceiveActor
        {
            /// <summary>
            /// The cluster.
            /// </summary>
            private Akka.Cluster.Cluster cluster;

            /// <summary>
            /// User overridable callback.
            ///                 <p/>
            ///                 Is called when an Actor is started.
            ///                 Actors are automatically started asynchronously when created.
            ///                 Empty default implementation.
            /// </summary>
            protected override void PreStart()
            {
                this.cluster = Cluster.Get(Context.System);
            }

            public LogActor()
            {
                this.Receive<string>(m => this.Log(m));
            }

            private void Log(string message)
            {
                Context.GetLogger().Info("LOG {Port}: {StringMessage}", this.cluster.SelfAddress.Port, message);
            }
        }
    }
}