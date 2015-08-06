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
    using System.Security.Cryptography.X509Certificates;
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
        // [Fact]
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
                "seed1",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });
            StartSystem(
                2552,
                "seed2",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });
            StartSystem(
                0,
                "worker1",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "xunit" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });
            StartSystem(
                0,
                "worker2",
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
        // [Fact]
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
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 5s"));

            var systems = new List<ActorSystem>();
            var systemUpWaitHandles = new List<EventWaitHandle>();

            DateTimeOffset now = DateTimeOffset.Now;

            var sys1 = StartSystem(
                2551,
                "seed1",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });

            var loggerProps = Props.Create(() => new LogActor());

            var logger = sys1.ActorOf(loggerProps, "log");

            var sys2 = StartSystem(
                2552,
                "seed2",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys2.ActorOf(loggerProps, "log");

            var sys3 = StartSystem(
                0,
                "worker1",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys3.ActorOf(loggerProps, "log");

            var sys4 = StartSystem(
                0,
                "worker2",
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys4.ActorOf(loggerProps, "log");

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            Log.Information("Cluster is UP and waiting ({Ms} for start)", (DateTimeOffset.Now - now).TotalMilliseconds);

            sys1.ActorSelection("/user/log").Tell("Hello World");
            sys2.ActorSelection("/user/log").Tell("Hello World");
            sys3.ActorSelection("/user/log").Tell("Hello World");
            sys4.ActorSelection("/user/log").Tell("Hello World");

            sys1.ActorSelection("/user/log").Tell(logger);
            sys2.ActorSelection("/user/log").Tell(logger);
            sys1.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2552/user/log").Tell(logger);

            sys1.ActorSelection("/user/log").Tell(loggerProps);
            sys1.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2552/user/log").Tell(loggerProps);

            sys3.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2551/user/log").Tell("Hello World remote");
            sys4.ActorSelection("akka.tcp://ClusterSystem@127.0.0.1:2551/user/log").Tell("Hello World remote");

            Thread.Sleep(TimeSpan.FromSeconds(2));

            /*
            var sys2Router = sys2.ActorOf(
                    Props.Empty.WithRouter(new ClusterRouterGroup(new ConsistentHashingGroup("/user/log"),
                        new ClusterRouterGroupSettings(100, false, "role2", ImmutableHashSet.Create("/user/log")))), "logRouter");

            var sys1Router = sys1.ActorOf(Props.Empty.WithRouter(new ClusterRouterGroup(new ConsistentHashingGroup("/user/log"),
                        new ClusterRouterGroupSettings(100, false, "role2", ImmutableHashSet.Create("/user/log")))), "logRouter");

            Log.Information(sys2Router.Path.ToString());

            var keysCol = 100;
            SendClusterMessages(sys2Router, keysCol);
            SendClusterMessages(sys2Router, keysCol);

            SendClusterMessages(sys1Router, keysCol);
            SendClusterMessages(sys1Router, keysCol);

            sys4.Shutdown();
            sys4.AwaitTermination();
            systems.Remove(sys4);
            SendClusterMessages(sys2Router, keysCol);
            SendClusterMessages(sys1Router, keysCol);
            Thread.Sleep(TimeSpan.FromSeconds(5));
            SendClusterMessages(sys2Router, keysCol);
            SendClusterMessages(sys1Router, keysCol);

            systemUpWaitHandles.Clear();
            sys4 = StartSystem(
                            0,
                            "worker2-new",
                            baseConfig,
                            systems,
                            systemUpWaitHandles,
                            new[] { "role2" },
                            new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            sys4.ActorOf(loggerProps, "log");

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            SendClusterMessages(sys2Router, keysCol * 2);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            SendClusterMessages(sys2Router, keysCol * 3);

            sys3.Shutdown();
            sys3.AwaitTermination();
            systems.Remove(sys3);

            SendClusterMessages(sys2Router, keysCol * 2);
            Thread.Sleep(TimeSpan.FromSeconds(10));
            SendClusterMessages(sys2Router, keysCol * 3);

            // Thread.Sleep(TimeSpan.FromSeconds(2));
            */
            foreach (var system in systems)
            {
                system.Shutdown();
            }

            foreach (var system in systems)
            {
                system.AwaitTermination();
            }
        }

        private static void SendClusterMessages(IActorRef sys2Router, int keysCol)
        {
            Log.Information("------------------------");
            Log.Information("Starting sending cluster messages");

            var routees = sys2Router.Ask<Routees>(new GetRoutees()).Result;
            Log.Information($"Routees before start: {routees.Members.Count()}");
            foreach (var member in routees.Members)
            {
                member.Send("I am routee before start", ActorRefs.NoSender);
            }

            var recieves = new Dictionary<int, int>();
            int messagesLost = 0;

            var tasks = Enumerable.Range(0, 1000).Select(
                i => Task.Run(
                    async () =>
                        {
                            try
                            {
                                var port =
                                    await sys2Router.Ask<int>(
                                        new ConsistentHashableEnvelope(
                                            new LogActor.PingMessage { Message = $"Hello {i} {i % keysCol}" },
                                            i % keysCol),
                                        TimeSpan.FromMilliseconds(1000));

                                lock (recieves)
                                {
                                    if (!recieves.ContainsKey(port))
                                    {
                                        recieves[port] = 0;
                                    }

                                    recieves[port]++;
                                }
                            }
                            catch (Exception)
                            {
                                Interlocked.Add(ref messagesLost, 1);
                            }
                        }));

            Task.WaitAll(tasks.ToArray());

            if (messagesLost > 0)
            {
                Log.Error("{messagesLost} messages was lost", messagesLost);
            }

            foreach (var key in recieves.Keys)
            {
                Log.Information("{Port} recieved {Count} messages", key, recieves[key]);
            }

            routees = sys2Router.Ask<Routees>(new GetRoutees()).Result;
            Log.Information($"Routees after stop: {routees.Members.Count()}");
            foreach (var member in routees.Members)
            {
                member.Send("I am routee after stop", ActorRefs.NoSender);
            }
        }

        /// <summary>
        /// Actor test system start
        /// </summary>
        /// <param name="port">Network port</param>
        /// <param name="uid">Идентификатор ноды</param>
        /// <param name="baseConfig">Base configuration description</param>
        /// <param name="systems">Complet cluster node list (will add new system to it)</param>
        /// <param name="systemUpWaitHandles">List of wait handles for clusters initialization waiting</param>
        /// <param name="roles">Node roles list</param>
        /// <param name="seedNodes">Seed nodes addresses list</param>
        private static ActorSystem StartSystem(
            int port,
            string uid,
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
                    //.WithFallback(ConfigurationFactory.ParseString($"akka.cluster.uid = \"{uid}\""))
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

                this.cluster.RegisterOnMemberUp(() =>
                    { Context.GetLogger().Info("{Address}: I am up", this.cluster.SelfAddress.Port); });
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
                this.Receive<PingMessage>(m => this.Log(m));
                this.Receive<object>(m => this.Log(m));
            }

            private void Log(string message)
            {
                Context.GetLogger().Info("LOG {Port}: {StringMessage}", this.cluster.SelfAddress.Port, message);
            }

            private void Log(object message)
            {
                Context.GetLogger().Info("LOG {Port}: object {StringMessage}", this.cluster.SelfAddress.Port, message.GetType().Name);
            }

            private void Log(PingMessage message)
            {
                // Context.GetLogger().Info("LOG {Port}: {StringMessage}", this.cluster.SelfAddress.Port, message.Message);
                this.Sender.Tell(this.cluster.SelfAddress.Port);
            }

            public class PingMessage
            {
                public string Message { get; set; }
            }
        }
    }
}