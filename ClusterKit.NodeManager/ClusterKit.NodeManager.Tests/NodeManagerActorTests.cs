// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerActorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing node manager actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.TestKit;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.Data;
    using ClusterKit.Core.Data.TestKit;
    using ClusterKit.Core.EF;
    using ClusterKit.Core.EF.TestKit;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Messages;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing node manager actor
    /// </summary>
    public class NodeManagerActorTests : BaseActorTest<NodeManagerActorTests.Configurator>
    {
        /// <summary>
        /// Access to xunit output
        /// </summary>
        private ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NodeManagerActorTests(ITestOutputHelper output)
            : base(output)
        {
            this.output = output;
        }

        /// <summary>
        /// Tests actor start
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task ActorStartTest()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>());
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(1));
            Assert.NotNull(response);
        }

        /// <summary>
        /// Node description collection test
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task NodeDescriptionCollection()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>());

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            if (message is RemoteTestMessage<Identify>)
                            {
                                var identifyMessage = (RemoteTestMessage<Identify>)message;
                                sender.Tell(new ActorIdentity(identifyMessage.Message.MessageId, this.TestActor));
                            }

                            if (message is NodeDescriptionRequest)
                            {
                                sender.Tell(
                                    new NodeDescription
                                    {
                                        ContainerType = "test",
                                        Modules =
                                                new List<PackageDescription>
                                                    {
                                                        new PackageDescription
                                                            {
                                                                Id = "TestModule",
                                                                Version = "0.1.0"
                                                            }
                                                    },
                                        NodeAddress = nodeAddress.Address,
                                        NodeId = Guid.NewGuid(),
                                        NodeTemplate = "test-template",
                                        StartTimeStamp = 10
                                    });
                            }

                            return AutoPilot.KeepRunning;
                        }));

            testActor.Tell(
                new ClusterEvent.MemberUp(
                    Member.Create(nodeAddress, 1, MemberStatus.Up, ImmutableHashSet<string>.Empty)));
            this.ExpectMsg<Identify>("/user/NodeManager/Receiver");
            this.ExpectMsg<NodeDescriptionRequest>();

            var descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);
            Assert.Equal("test-template", descriptions[0].NodeTemplate);
        }

        /// <summary>
        /// Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            /// Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = new List<BaseInstaller> { new Core.TestKit.Installer(), new TestInstaller() };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// Replaces production datasources with the test ones
        /// </summary>
        public class TestInstaller : BaseInstaller
        {
            protected override decimal AkkaConfigLoadPriority => -1M;

            protected override Config GetAkkaConfig() => ConfigurationFactory.ParseString(@"
            {
                ClusterKit.NodeManager.ConfigurationDatabaseName = ""TestConfigurationDatabase""

                akka : {
                  stdout-loglevel : INFO
                  loggers : [""Akka.Logger.Serilog.SerilogLogger, Akka.Logger.Serilog""]
                  log - config - on - start : off
                  loglevel: INFO

                  actor: {
                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    default- dispatcher {
                      type = TaskDispatcher
                    }

                    deployment {
                      /Core {
                        IsNameSpace = true
                      }

                      /Core/Ping {
                        type = ""ClusterKit.Core.Ping.PingActor, ClusterKit.Core""
                      }
                    }

                    serializers {
                      akka-singleton = ""Akka.Cluster.Tools.Singleton.Serialization.ClusterSingletonMessageSerializer, Akka.Cluster.Tools""
                    }
                    serialization-bindings {
                      ""Akka.Cluster.Tools.Singleton.ClusterSingletonMessage, Akka.Cluster.Tools"" = akka-singleton
                    }
                    serialization-identifiers {
                      ""Akka.Cluster.Tools.Singleton.Serialization.ClusterSingletonMessageSerializer, Akka.Cluster.Tools"" = 14
                    }
                  }

                  remote : {
                    helios.tcp : {
                      hostname = 127.0.0.1
                      port = 0
                    }
                  }

                  cluster: {
                    auto-down-unreachable-after = 15s
                    seed-nodes = []
            singleton {
                        # The number of retries are derived from hand-over-retry-interval and
                        # akka.cluster.down-removal-margin (or ClusterSingletonManagerSettings.removalMargin),
                        # but it will never be less than this property.
                        min-number-of-hand-over-retries = 10
                    }
                  }
                }
            }");

            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(Classes.FromAssemblyContaining<NodeManagerActor>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());
                container.Register(Classes.FromAssemblyContaining<Core.Installer>().Where(t => t.IsSubclassOf(typeof(ActorBase))).LifestyleTransient());

                container.Register(Component.For<BaseConnectionManager>().Instance(new TestConnectionManager()).LifestyleSingleton());

                container.Register(Component.For<DataFactory<ConfigurationContext, NodeTemplate, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, NugetFeed, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, NugetFeed, int>(null, o => o.Id)).LifestyleSingleton());
                container.Register(Component.For<DataFactory<ConfigurationContext, SeedAddress, int>>()
                    .Instance(new UniversalTestDataFactory<ConfigurationContext, SeedAddress, int>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<DataFactory<string, PackageDescription, string>>()
                    .Instance(new UniversalTestDataFactory<string, PackageDescription, string>(null, o => o.Id)).LifestyleSingleton());

                container.Register(Component.For<IContextFactory<ConfigurationContext>>().Instance(new TestContextFactory<ConfigurationContext>()).LifestyleSingleton());
                container.Register(Component.For<IMessageRouter>().ImplementedBy<TestMessageRouter>().LifestyleTransient());
            }
        }
    }
}