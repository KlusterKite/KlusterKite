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
    using ClusterKit.Core.Rest.ActionMessages;
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
        /// Initializes a new instance of the <see cref="NodeManagerActorTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public NodeManagerActorTests(ITestOutputHelper output)
            : base(output)
        {
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
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(1));
            Assert.NotNull(response);
        }

        /// <summary>
        /// Tests node is obsolete on start
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task ClusterUpgradeTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
            await packageFactory.Insert(new PackageDescription { Id = "TestModule-2", Version = "0.1.0" });
            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template",
                        Code = "test-template",
                        Id = 1,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 1
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var virtualNodes = new List<VirtualNode>()
                                   {
                                       new VirtualNode(1, this.Sys),
                                       new VirtualNode(2, this.Sys),
                                       new VirtualNode(3, this.Sys),
                                   };

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        var identifyMessage = message as RemoteTestMessage<Identify>;
                        if (identifyMessage != null)
                        {
                            virtualNodes
                            .FirstOrDefault(n => n.Address.Address == identifyMessage.RecipientAddress)
                            ?.Client.Tell(identifyMessage.Message, sender);
                        }

                        var shutDownMessage = message as RemoteTestMessage<ShutdownMessage>;
                        if (shutDownMessage != null)
                        {
                            virtualNodes
                            .FirstOrDefault(n => n.Address.Address == shutDownMessage.RecipientAddress)
                            ?.Client.Tell(shutDownMessage.Message, sender);
                        }

                        return AutoPilot.KeepRunning;
                    }));

            foreach (var virtualNode in virtualNodes)
            {
                testActor.Tell(
                    new ClusterEvent.MemberUp(
                        Member.Create(virtualNode.Address, 1, MemberStatus.Up, ImmutableHashSet<string>.Empty)));
            }

            this.ExpectMsg<Identify>("/user/NodeManager/Receiver");
            this.ExpectMsg<NodeDescriptionRequest>();

            var descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);
            Assert.Equal(true, descriptions[0].IsObsolete);
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
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                        {
                            var identifyMessage = message as RemoteTestMessage<Identify>;
                            if (identifyMessage != null)
                            {
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
        /// Tests taht nodes become obsolete on nodetemplate edit
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task NodeIsObsoleteOnNodeTemplateEditTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
            await packageFactory.Insert(new PackageDescription { Id = "TestModule-2", Version = "0.1.0" });
            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template",
                        Code = "test-template",
                        Id = 1,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 1
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);
            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        var identifyMessage = message as RemoteTestMessage<Identify>;
                        if (identifyMessage != null)
                        {
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
                                                                Id = "TestModule-1",
                                                                Version = "0.1.0"
                                                            }
                                                },
                                    NodeAddress = nodeAddress.Address,
                                    NodeId = Guid.NewGuid(),
                                    NodeTemplate = "test-template",
                                    StartTimeStamp = 10,
                                    NodeTemplateVersion = 0
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
            Assert.Equal(false, descriptions[0].IsObsolete);

            var nodeTemplate = templatesFactory.Storage[1];
            testActor.Tell(new RestActionMessage<NodeTemplate, int> { ActionType = EnActionType.Update, Request = nodeTemplate });

            //nodeTemplate.Version = 2;
            //testActor.Tell(new UpdateMessage<NodeTemplate> { ActionType = EnActionType.Update, NewObject = nodeTemplate, OldObject = nodeTemplate });
            descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);
            Assert.Equal(true, descriptions[0].IsObsolete);
        }

        /// <summary>
        /// Tests taht nodes become obsolete on new package version upload
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task NodeIsObsoleteOnPackageUpgradeTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
            await packageFactory.Insert(new PackageDescription { Id = "TestModule-2", Version = "0.1.0" });
            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template",
                        Code = "test-template",
                        Id = 1,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 1
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);
            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        var identifyMessage = message as RemoteTestMessage<Identify>;
                        if (identifyMessage != null)
                        {
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
                                                                Id = "TestModule-1",
                                                                Version = "0.1.0"
                                                            }
                                                },
                                    NodeAddress = nodeAddress.Address,
                                    NodeId = Guid.NewGuid(),
                                    NodeTemplate = "test-template",
                                    StartTimeStamp = 10,
                                    NodeTemplateVersion = 0
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
            Assert.Equal(false, descriptions[0].IsObsolete);

            packageFactory.Storage["TestModule-1"].Version = "0.2.0";
            testActor.Tell(new ReloadPackageListRequest());
            descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);
            Assert.Equal(true, descriptions[0].IsObsolete);
        }

        /// <summary>
        /// Tests node is obsolete on start
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task NodeIsObsoleteOnStartTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.2.0" });
            await packageFactory.Insert(new PackageDescription { Id = "TestModule-2", Version = "0.1.0" });
            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template",
                        Code = "test-template",
                        Id = 1,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 1
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);
            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate (IActorRef sender, object message)
                    {
                        var identifyMessage = message as RemoteTestMessage<Identify>;
                        if (identifyMessage != null)
                        {
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
                                                                Id = "TestModule-1",
                                                                Version = "0.1.0"
                                                            }
                                                },
                                    NodeAddress = nodeAddress.Address,
                                    NodeId = Guid.NewGuid(),
                                    NodeTemplate = "test-template",
                                    StartTimeStamp = 10,
                                    NodeTemplateVersion = 0
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
            Assert.Equal(true, descriptions[0].IsObsolete);
        }

        /// <summary>
        /// Tests actor initialization
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task PackageListRequestTest()
        {
            /*
            var templatesFactory = (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var addressFactory = (UniversalTestDa taFactory<ConfigurationContext, SeedAddress, int>)this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, SeedAddress, int>>();
            var feedFactory = (UniversalTestDataFactory<ConfigurationContext, NugetFeed, int>)this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NugetFeed, int>>();
            */
            var packageFactory = (UniversalTestDataFactory<string, PackageDescription, string>)this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModeule-1", Version = "0.1.0" });
            await packageFactory.Insert(new PackageDescription { Id = "TestModeule-2", Version = "0.1.0" });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var response = await testActor.Ask<List<PackageDescription>>(new PackageListRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(response);
            Assert.Equal(2, response.Count);
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
        private class TestInstaller : BaseInstaller
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

                    /nodemanager/workers {
                        router = consistent-hashing-pool
                        nr-of-instances = 5
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

        private class VirtualNode
        {
            public VirtualNode(int num, ActorSystem system)
            {
                this.Number = num;
                this.NodeId = Guid.NewGuid();
                this.Address = new UniqueAddress(new Address("akka.tcp", "ClusterKit", $"testNode{num}", num), num);
                this.Description = new NodeDescription
                {
                    ContainerType = "test",
                    Modules =
                                               new List<PackageDescription>
                                                   {
                                                       new PackageDescription
                                                           {
                                                               Id
                                                                   =
                                                                   "TestModule-1",
                                                               Version
                                                                   =
                                                                   "0.1.0"
                                                           }
                                                   },
                    NodeAddress = this.Address.Address,
                    NodeId = this.NodeId,
                    NodeTemplate = "test-template",
                    StartTimeStamp = 10,
                    NodeTemplateVersion = 0
                };

                this.Client = system.ActorOf(Props.Create(() => new VirtualClient(this)), $"virtualNode{num}");
            }

            public UniqueAddress Address { get; set; }

            public IActorRef Client { get; set; }

            public NodeDescription Description { get; set; }

            public Guid NodeId { get; set; }

            public int Number { get; set; }

            public class VirtualClient : ReceiveActor
            {
                private VirtualNode node;

                public VirtualClient(VirtualNode node)
                {
                    this.node = node;

                    this.Receive<Identify>(m => this.Sender.Tell(new ActorIdentity(m.MessageId, this.Self)));
                    this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(this.node.Description, this.Self));
                }
            }
        }
    }
}