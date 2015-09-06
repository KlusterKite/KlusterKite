using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxiKit.Core.Tests.BusinessObjects
{
    using System.Threading;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.Event;
    using Akka.Logger.Serilog;

    using StackExchange.Redis;

    using TaxiKit.Core.Cluster;
    using TaxiKit.Core.TestKit;
    using TaxiKit.Core.Tests.ConceptProof;

    using Xunit;
    using Xunit.Abstractions;

    using static Serilog.Log;

    /// <summary>
    /// Testing <seealso cref="ClusterBusinessObjectActorSupervisor{T}"/>
    /// </summary>
    public class ClusterBusinessObjectActorSupervisorTest : TestWithSerilog
    {
        public ClusterBusinessObjectActorSupervisorTest(ITestOutputHelper output)
                    : base(output)
        {
        }

        /// <summary>
        /// Testinf system start
        /// </summary>
        [Fact]
        public void SystemStartTest()
        {
            var baseConfig =
                ConfigurationFactory.Empty.WithFallback(
                    ConfigurationFactory.ParseString(
                        "akka.actor.provider = \"Akka.Cluster.ClusterActorRefProvider, Akka.Cluster\""))
                    .WithFallback(
                        ConfigurationFactory.ParseString(
                            "akka.loggers = [\"Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog\"]"))
                    .WithFallback(ConfigurationFactory.ParseString("min-nr-of-members = 2"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.loglevel = INFO"))
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 1s"));

            var systemUpWaitHandles = new List<EventWaitHandle>();
            var systems = new List<ActorSystem>();

            var sys1 = StartSystem(
                2551,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });

            var sys2 = StartSystem(
                2552,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1", "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            var sys3 = StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1", "role2" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            Logger.Information("***************************** STARTED ***********************");

            sys2.Shutdown();
            sys2.AwaitTermination();

            Thread.Sleep(TimeSpan.FromMilliseconds(1000 * 10));
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
            system.ActorOf(Props.Create(() => new ClusterWaitActor(waitHandle)), "ClusterWait");
            systemUpWaitHandles.Add(waitHandle);

            return system;
        }

        /// <summary>
        /// Test message to BO actor
        /// </summary>
        public class LogMessage : IMessageToBusinessObjectActor
        {
            public string Id { get; set; }
        }

        public class TestObjectActor : ReceiveActor
        {
        }

        public class TestSuperVisor : ClusterBusinessObjectActorSupervisor<TestObjectActor>
        {
            public TestSuperVisor(IConnectionMultiplexer redisConnection)
                            : base(redisConnection)
            {
            }

            /// <summary>
            /// Cluster node role name, that handles such objects.
            /// </summary>
            protected override string ClusterRole => "bo";
        }

        /// <summary>
        /// Actor for cluster build lookup
        /// </summary>
        private class ClusterWaitActor : ReceiveActor
        {
            private readonly EventWaitHandle nodesUp;

            /// <summary>
            /// The cluster.
            /// </summary>
            private Akka.Cluster.Cluster cluster;

            /// <summary>
            /// Current known members (And I always know myself)
            /// </summary>
            private int membersCount = 0;

            public ClusterWaitActor(EventWaitHandle nodesUp)
            {
                this.nodesUp = nodesUp;

                this.Receive<ClusterEvent.IClusterDomainEvent>(m => this.LogClusterEvent(m));
            }

            /// <summary>
            /// Re-subscribe on restart
            /// </summary>
            protected override void PostStop()
            {
                this.cluster.Unsubscribe(this.Self);
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
                    new[] { typeof(ClusterEvent.IClusterDomainEvent) });

                this.cluster.RegisterOnMemberUp(
                    () =>
                        {
                            Context.GetLogger().Info("{Address}: I am up", this.cluster.SelfUniqueAddress.Uid);
                            this.nodesUp.Set();
                        });
            }

            private bool LogClusterEvent(ClusterEvent.IClusterDomainEvent message)
            {
                var leaderChanged = message as ClusterEvent.LeaderChanged;
                var roleLeaderChanged = message as ClusterEvent.RoleLeaderChanged;
                var memberUp = message as ClusterEvent.MemberUp;

                if (leaderChanged != null)
                {
                    Context.GetLogger(new SerilogLogMessageFormatter())
                        .Info(
                            "{Address}: Cluster leader changed to {leader}",
                            this.cluster.SelfUniqueAddress.Uid,
                            leaderChanged.Leader.Port);
                }
                else if (roleLeaderChanged != null)
                {
                    Context.GetLogger(new SerilogLogMessageFormatter())
                        .Info(
                            "{Address}: Role {role} leader changed to {leader}",
                            this.cluster.SelfUniqueAddress.Uid,
                            roleLeaderChanged.Role,
                            roleLeaderChanged.Leader.Port);
                }
                else if (memberUp != null)
                {
                    Context.GetLogger(new SerilogLogMessageFormatter())
                        .Info(
                            "{Address}: memberUp  {member}",
                            this.cluster.SelfUniqueAddress.Uid,
                            memberUp.Member.Address.Port);
                }
                else
                {
                    Context.GetLogger(new SerilogLogMessageFormatter())
                        .Info(
                            "{Address}: Cluster changed with {message}",
                            this.cluster.SelfUniqueAddress.Uid,
                            message);
                }

                return false;
            }
        }
    }
}