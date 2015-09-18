using System;
using System.Collections.Generic;
using System.Linq;

namespace TaxiKit.Core.Tests.BusinessObjects
{
    using System.Collections.Concurrent;
    using System.Threading;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.CastleWindsor;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.Logger.Serilog;

    using Castle.MicroKernel.Registration;
    using Castle.Windsor;

    using JetBrains.Annotations;

    using StackExchange.Redis;

    using TaxiKit.Core.Cluster;
    using TaxiKit.Core.TestKit;
    using TaxiKit.Core.TestKit.Moq;

    using Xunit;
    using Xunit.Abstractions;

    using static Serilog.Log;

    /// <summary>
    /// Testing <seealso cref="ClusterBusinessObjectActorSupervisor{T}"/>
    /// </summary>
    public class ClusterBusinessObjectActorSupervisorMultiSystemTest : TestWithSerilog
    {
        private readonly ConcurrentBag<EchoMessage> recievedEchoMessages = new ConcurrentBag<EchoMessage>();
        private WindsorContainer container;

        public ClusterBusinessObjectActorSupervisorMultiSystemTest(ITestOutputHelper output)
                    : base(output)
        {
            this.container = new WindsorContainer();
            this.container.RegisterWindsorInstallers();

            var redis = new ConcurrentDictionary<string, object>();

            this.container.Register(Component.For<IConnectionMultiplexer>().Instance(new RedisConnectionMoq(redis)));
            this.container.Register(Component.For<TestNetSupervisorActor>().LifestyleTransient());
            this.container.Register(Component.For<TestObjectActor>().LifestyleTransient());
            this.container.Register(Component.For<ITestOutputHelper>().Instance(output));
            this.container.Register(Component.For<IActorRef>().Instance(ActorRefs.Nobody).Named("testActor"));
            this.container.Register(Component.For<ConcurrentBag<EchoMessage>>().Instance(this.recievedEchoMessages));
        }

        /// <summary>
        /// Testinf system start
        /// </summary>
        // [Fact]
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
                    .WithFallback(ConfigurationFactory.ParseString("akka.cluster.auto-down-unreachable-after = 1s"))
                    .WithFallback(ConfigurationFactory.ParseString(@" akka.actor.deployment {
                        /sup {
                            createChildTimeout = 1s
                            sendTimeOut = 300ms
                            nextAttmeptPause = 1s
                            sendersCount = 20
                        }
                    }"));

            var systemUpWaitHandles = new List<EventWaitHandle>();
            var systems = new List<ActorSystem>();

            var sys1 = this.StartSystem(
                2551,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1", "test" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551" });

            var s1 = sys1.ActorOf(sys1.DI().Props<TestNetSupervisorActor>(), "sup");

            var sys2 = this.StartSystem(
                2552,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1", "test" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            var s2 = sys2.ActorOf(sys1.DI().Props<TestNetSupervisorActor>(), "sup");

            var sys3 = this.StartSystem(
                0,
                baseConfig,
                systems,
                systemUpWaitHandles,
                new[] { "role1", "test" },
                new[] { "akka.tcp://ClusterSystem@127.0.0.1:2551", "akka.tcp://ClusterSystem@127.0.0.1:2552" });

            var s3 = sys3.ActorOf(sys1.DI().Props<TestNetSupervisorActor>(), "sup");

            foreach (var systemUpWaitHandle in systemUpWaitHandles)
            {
                Assert.True(systemUpWaitHandle.WaitOne(TimeSpan.FromSeconds(10)), "Cluster failed to be built");
            }

            Logger.Information("***************************** STARTED ***********************");

            int messagesCount = 100;
            foreach (var index in Enumerable.Range(0, messagesCount))
            {
                s1.Tell(new EchoMessage { Id = (index % 20).ToString(), Text = index.ToString() });
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(1500));

            Assert.Equal(messagesCount, this.recievedEchoMessages.Count);

            sys2.Shutdown();
            sys2.AwaitTermination();

            foreach (var index in Enumerable.Range(0, messagesCount))
            {
                s1.Tell(new EchoMessage { Id = (index % 20).ToString(), Text = index.ToString() });
            }

            Thread.Sleep(TimeSpan.FromMilliseconds(15000));

            Assert.True(messagesCount * 2 <= this.recievedEchoMessages.Count);
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
        private ActorSystem StartSystem(
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
            system.AddDependencyResolver(new WindsorDependencyResolver(this.container, system));
            systems.Add(system);
            Logger.Information("node {0} is up", port);
            var waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            system.ActorOf(Props.Create(() => new ClusterWaitActor(waitHandle)), "ClusterWait");
            systemUpWaitHandles.Add(waitHandle);

            return system;
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

    [UsedImplicitly]
    public class TestNetSupervisorActor : ClusterBusinessObjectActorSupervisor<TestObjectActor>
    {
        public TestNetSupervisorActor(IConnectionMultiplexer redisConnection)
            : base(redisConnection)
        {
        }

        protected override string ClusterRole => "test";
    }
}