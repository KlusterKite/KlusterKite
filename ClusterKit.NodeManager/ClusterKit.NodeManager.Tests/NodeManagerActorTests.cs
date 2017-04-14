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
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.TestKit;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Data;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.EF;
    using ClusterKit.Data.EF.Effort;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;

    using JetBrains.Annotations;

    using NuGet;

    using Xunit;
    using Xunit.Abstractions;

    using Installer = ClusterKit.Core.TestKit.Installer;

    /// <summary>
    ///     Testing node manager actor
    /// </summary>
    public class NodeManagerActorTests : BaseActorTest<NodeManagerActorTests.Configurator>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NodeManagerActorTests" /> class.
        /// </summary>
        /// <param name="output">
        ///     The output.
        /// </param>
        public NodeManagerActorTests(ITestOutputHelper output)
            : base(output)
        {
        }

        /// <summary>
        ///     Tests actor start
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task ActorStartTest()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(10));
            Assert.NotNull(response);
        }

        /// <summary>
        ///     Tests node is obsolete on start
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        /// TODO: this
        [Fact]
        public async Task ClusterUpgradeTest()
        {
            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            this.ExpectNoMsg();
            var virtualNodes = new List<VirtualNode>
                                   {
                                       new VirtualNode(1, testActor, this.Sys, 1, "t1"),
                                       new VirtualNode(2, testActor, this.Sys, 1, "t1"),
                                       new VirtualNode(3, testActor, this.Sys, 1, "t1")
                                   };

            foreach (var n in virtualNodes)
            {
                router.RegisterVirtualNode(n.Address.Address, n.Client);
                n.GoUp();
            }

            this.ExpectNoMsg();
            this.Log.Warning("!!!!!!!!!!! Nodes started");

            var descriptions = await testActor.Ask<List<NodeDescription>>(
                                   new ActiveNodeDescriptionsRequest(),
                                   TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(3, descriptions.Count);
            Assert.True(descriptions.All(d => !d.IsObsolete));

            // defining new release
            var newRelease = ReleaseCheckTestsBase.CreateRelease();
            newRelease.Configuration.SeedAddresses = new List<string> { virtualNodes[0].Address.ToString() };
            newRelease.Configuration.NugetFeeds = new List<NugetFeed> { new NugetFeed { Address = "url" } };
            newRelease.Configuration.NodeTemplates[0].MinimumRequiredInstances = 2;
            newRelease.Configuration.Packages = ReleaseCheckTestsBase
                .CreatePackageDescriptions("p1 2.0.0", "dp1 2.0.0", "p2 2.0.0", "dp2 2.0.0")
                .ToList();

            // saving new release
            var response = await testActor.Ask<CrudActionResponse<Release>>(
                               new CrudActionMessage<Release, int>
                                   {
                                       ActionType = EnActionType.Create,
                                       Data = newRelease
                                   },
                               TimeSpan.FromSeconds(1));

            Assert.NotNull(response.Data);
            var newReleaseId = response.Data.Id;
            Assert.Equal(2, newReleaseId);
            this.ExpectNoMsg();

            // setting state Ready to the new release
            response = await testActor.Ask<CrudActionResponse<Release>>(
                           new ReleaseSetReadyRequest { Id = newReleaseId },
                           TimeSpan.FromSeconds(1));
            Assert.NotNull(response.Data);
            this.ExpectNoMsg();

            // requesting cluster upgrade
            response = await testActor.Ask<CrudActionResponse<Release>>(
                           new UpdateClusterRequest { Id = newReleaseId },
                           TimeSpan.FromSeconds(1));
            Assert.NotNull(response.Data);
            Assert.Equal(EnReleaseState.Active, response.Data.State);
            this.ExpectNoMsg();
            this.Log.Warning("!!!!!!!!!!! Cluster upgrade initiated");

            descriptions = await testActor.Ask<List<NodeDescription>>(
                               new ActiveNodeDescriptionsRequest(),
                               TimeSpan.FromSeconds(5));

            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(2, virtualNodes.Count(n => n.IsUp));
            Assert.True(descriptions.All(d => d.IsObsolete));

            this.ExpectNoMsg();

            var virtualNode = virtualNodes.First(n => n.Number == 1);
            Assert.False(virtualNode.IsUp); // the oldest node gone to upgrade

            virtualNode.Description.StartTimeStamp = VirtualNode.GetNextTime();
            virtualNode.Description.ReleaseId = newReleaseId;
            virtualNode.GoUp();

            this.ExpectNoMsg();

            descriptions = await testActor.Ask<List<NodeDescription>>(
                               new ActiveNodeDescriptionsRequest(),
                               TimeSpan.FromSeconds(1));

            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(2, virtualNodes.Count(n => n.IsUp));
            Assert.Equal(1, descriptions.Count(d => d.IsObsolete));
        }

        /// <summary>
        ///     Node description collection test
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task NodeDescriptionCollection()
        {
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "ClusterKit", "testNode1", 1), 1);

            this.SetAutoPilot(
                new DelegateAutoPilot(delegate(IActorRef sender, object message)
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
            this.ExpectNoMsg();
            testActor.Tell(
                new ClusterEvent.MemberUp(
                    ClusterExtensions.MemberCreate(nodeAddress, 1, MemberStatus.Up, ImmutableHashSet<string>.Empty)));

            this.ExpectMsg<Identify>("/user/NodeManager/Receiver");
            this.ExpectMsg<NodeDescriptionRequest>();

            var descriptions = await testActor.Ask<List<NodeDescription>>(
                                   new ActiveNodeDescriptionsRequest(),
                                   TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(1, descriptions.Count);
            Assert.Equal("test-template", descriptions[0].NodeTemplate);
        }

        /// <summary>
        ///     Tests node is obsolete on start
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task NodeIsObsoleteOnStartTest()
        {
            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            this.ExpectNoMsg();
            var activeNode = new VirtualNode(1, testActor, this.Sys, 1);
            var obsoleteNode = new VirtualNode(2, testActor, this.Sys, 2);
            router.RegisterVirtualNode(activeNode.Address.Address, activeNode.Client);
            router.RegisterVirtualNode(obsoleteNode.Address.Address, obsoleteNode.Client);
            activeNode.GoUp();
            obsoleteNode.GoUp();
            this.ExpectNoMsg();

            var descriptions = await testActor.Ask<List<NodeDescription>>(
                                   new ActiveNodeDescriptionsRequest(),
                                   TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(
                false,
                descriptions.FirstOrDefault(d => d.NodeAddress == activeNode.Address.Address)?.IsObsolete);
            Assert.Equal(
                true,
                descriptions.FirstOrDefault(d => d.NodeAddress == obsoleteNode.Address.Address)?.IsObsolete);
        }

        /// <summary>
        ///     Tests node is obsolete on start
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task NodeWithCompatibleTemplateTest()
        {
            using (var context = this.GetContext())
            {
                var oldRelease = context.Releases.First(r => r.State == EnReleaseState.Active);
                oldRelease.State = EnReleaseState.Obsolete;

                var newRelease = ReleaseCheckTestsBase.CreateRelease();
                newRelease.State = EnReleaseState.Active;
                newRelease.Configuration.SeedAddresses = new List<string>();
                newRelease.Configuration.NugetFeeds = new List<NugetFeed>();
                newRelease.Configuration.NodeTemplates[0].MinimumRequiredInstances = 2;
                newRelease.Configuration.NodeTemplates[0].PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            [ReleaseCheckTestsBase.Net45] =
                            new List<PackageDescription>(
                                newRelease.Configuration.Packages)
                        };

                newRelease.CompatibleTemplates =
                    new List<CompatibleTemplate>
                        {
                            new CompatibleTemplate
                                {
                                    CompatibleReleaseId = 1,
                                    TemplateCode = "t1"
                                }
                        };

                context.Releases.Add(newRelease);
                context.SaveChanges();
            }

            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            this.ExpectNoMsg();
            var activeNode = new VirtualNode(1, testActor, this.Sys, 1);
            var obsoleteNode = new VirtualNode(2, testActor, this.Sys, 2);
            router.RegisterVirtualNode(activeNode.Address.Address, activeNode.Client);
            router.RegisterVirtualNode(obsoleteNode.Address.Address, obsoleteNode.Client);
            activeNode.GoUp();
            obsoleteNode.GoUp();
            this.ExpectNoMsg();

            var descriptions = await testActor.Ask<List<NodeDescription>>(
                                   new ActiveNodeDescriptionsRequest(),
                                   TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(
                false,
                descriptions.FirstOrDefault(d => d.NodeAddress == activeNode.Address.Address)?.IsObsolete);
            Assert.Equal(
                false,
                descriptions.FirstOrDefault(d => d.NodeAddress == obsoleteNode.Address.Address)?.IsObsolete);
        }

        /// <summary>
        ///     Tests template selection
        /// </summary>
        /// <returns>
        ///     The <see cref="Task" />.
        /// </returns>
        [Fact]
        public async Task TemplateSelectionTest()
        {
            using (var context = this.GetContext())
            {
                var release = context.Releases.First(r => r.State == EnReleaseState.Active);

                var template1 = ReleaseCheckTestsBase.CreateRelease().Configuration.NodeTemplates.First();
                template1.Code = "t1";
                template1.Priority = 1;
                template1.PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            {
                                ReleaseCheckTestsBase.Net45,
                                new List<PackageDescription>()
                            }
                        };

                var template2 = ReleaseCheckTestsBase.CreateRelease().Configuration.NodeTemplates.First();
                template2.Code = "t2";
                template2.Priority = 1000000;
                template2.PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            {
                                ReleaseCheckTestsBase.Net45,
                                new List<PackageDescription>()
                            }
                        };

                release.Configuration.NodeTemplates = new[] { template1, template2 }.ToList();
                context.SaveChanges();
            }

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var templates = await testActor.Ask<List<NodeTemplate>>(
                                new AvailableTemplatesRequest
                                    {
                                        ContainerType = "test",
                                        FrameworkRuntimeType = ReleaseCheckTestsBase.Net45
                                    },
                                TimeSpan.FromSeconds(1));
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var description = await testActor.Ask<NodeStartUpConfiguration>(
                                  new NewNodeTemplateRequest
                                      {
                                          ContainerType = "test",
                                          FrameworkRuntimeType = ReleaseCheckTestsBase.Net45,
                                          NodeUid = Guid.NewGuid()
                                      },
                                  TimeSpan.FromSeconds(1));
            Assert.NotNull(description);
            Assert.Equal("t2", description.NodeTemplate); // we have 1 in million chance of false failure
        }

        /// <summary>
        ///     Creates database context
        /// </summary>
        /// <returns>The database context</returns>
        private ConfigurationContext GetContext()
        {
            var config = this.Sys.Settings.Config;
            var connectionString = config.GetString("ClusterKit.NodeManager.ConfigurationDatabaseConnectionString");
            return this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>()
                .CreateContext(connectionString, string.Empty)
                .Result;
        }

        /// <summary>
        ///     Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            ///     Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = new List<BaseInstaller> { new Installer(), new TestInstaller() };
                return pluginInstallers;
            }
        }

        /// <summary>
        ///     A <see cref="ConfigurationContext" /> factory with test seeding
        /// </summary>
        private class ContextFactory : EffortContextFactory<ConfigurationContext>
        {
            /// <inheritdoc />
            public ContextFactory([NotNull] BaseConnectionManager connectionManager)
                : base(connectionManager)
            {
            }

            /// <inheritdoc />
            public override async Task<ConfigurationContext> CreateContext(
                string connectionString,
                string databaseName)
            {
                var context = await base.CreateContext(connectionString, databaseName);
                if (!context.Releases.Any())
                {
                    var release = ReleaseCheckTestsBase.CreateRelease();
                    release.State = EnReleaseState.Active;
                    release.Configuration.SeedAddresses = new List<string>();
                    release.Configuration.NugetFeeds = new List<NugetFeed>();
                    release.Configuration.NodeTemplates[0].MinimumRequiredInstances = 2;
                    release.Configuration.NodeTemplates[0].PackagesToInstall =
                        new Dictionary<string, List<PackageDescription>>
                            {
                                [ReleaseCheckTestsBase.Net45] =
                                new List<PackageDescription>(
                                    release.Configuration.Packages)
                            };
                    context.Releases.Add(release);
                    context.SaveChanges();
                }

                return context;
            }
        }

        /// <summary>
        ///     Replaces production data sources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => -1M;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return ConfigurationFactory.ParseString(
                    $@"
            {{
                ClusterKit.NodeManager.ConfigurationDatabaseName = """"
                ClusterKit.NodeManager.ConfigurationDatabaseConnectionString = ""{Guid.NewGuid():N}""

                akka : {{
                  actor: {{
                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    deployment {{
                        /nodemanager {{
                            dispatcher = akka.test.calling-thread-dispatcher
                        }}
                        /nodemanager/workers {{
                            router = consistent-hashing-pool
                            nr-of-instances = 5
                            dispatcher = akka.test.calling-thread-dispatcher
                        }}
                    }}

                    serializers {{
		                hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                    }}
                    serialization-bindings {{
                        ""System.Object"" = hyperion
                    }}
                  }}

                    remote : {{
                        helios.tcp : {{
                          hostname = 127.0.0.1
                          port = 0
                        }}
                      }}

                      cluster: {{
                        auto-down-unreachable-after = 15s
		                min-nr-of-members = 3
                        seed-nodes = []
                        singleton {{
                            # The number of retries are derived from hand-over-retry-interval and
                            # akka.cluster.down-removal-margin (or ClusterSingletonManagerSettings.removalMargin),
                            # but it will never be less than this property.
                            min-number-of-hand-over-retries = 10
                        }}
                      }}
                }}
            }}");
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(
                    Classes.FromAssemblyContaining<NodeManagerActor>()
                        .Where(t => t.IsSubclassOf(typeof(ActorBase)))
                        .LifestyleTransient());
                container.Register(
                    Classes.FromAssemblyContaining<Core.Installer>()
                        .Where(t => t.IsSubclassOf(typeof(ActorBase)))
                        .LifestyleTransient());

                container.Register(
                    Component.For<BaseConnectionManager>().Instance(new ConnectionManager()).LifestyleSingleton());
                container.Register(
                    Component.For<IContextFactory<ConfigurationContext>>()
                        .ImplementedBy<ContextFactory>()
                        .LifestyleTransient());

                container.Register(
                    Component.For<DataFactory<ConfigurationContext, Release, int>>()
                        .ImplementedBy<ReleaseDataFactory>()
                        .LifestyleTransient());

                var packageRepository = this.CreateTestRepository();

                container.Register(Component.For<IPackageRepository>().Instance(packageRepository));
                container.Register(
                    Component.For<IMessageRouter>().ImplementedBy<TestMessageRouter>().LifestyleSingleton());
            }

            /// <summary>
            ///     Creates the test repository
            /// </summary>
            /// <returns>The test repository</returns>
            private IPackageRepository CreateTestRepository()
            {
                var p1 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p1",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp1 1.0.0")
                                    }
                        };

                var p2 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p2",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp2 1.0.0")
                                    }
                        };

                var p3 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p3",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp3 2.0.0")
                                    }
                        };
                var dp1 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp1",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                var dp2 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp2",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                var dp3 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp3",
                            Version = SemanticVersion.Parse("1.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                var p12 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p1",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp1 2.0.0")
                                    }
                        };

                var p22 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p2",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp2 2.0.0")
                                    }
                        };

                var p32 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "p3",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets =
                                new[]
                                    {
                                        ReleaseCheckTestsBase
                                            .CreatePackageDependencySet(
                                                ReleaseCheckTestsBase.Net45,
                                                "dp3 2.0.0")
                                    }
                        };
                var dp12 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp1",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                var dp22 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp2",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                var dp32 =
                    new ReleaseCheckTestsBase.TestPackage
                        {
                            Id = "dp3",
                            Version = SemanticVersion.Parse("2.0.0"),
                            DependencySets = new PackageDependencySet[0]
                        };

                return new ReleaseCheckTestsBase.TestRepository(
                    p1,
                    p2,
                    p3,
                    dp1,
                    dp2,
                    dp3,
                    p12,
                    p22,
                    p32,
                    dp12,
                    dp22,
                    dp32);
            }
        }

        /// <summary>
        ///     Virtual node to emulate cluster node responses to update
        /// </summary>
        private class VirtualNode
        {
            /// <summary>
            ///     Current virtual timestamp
            /// </summary>
            private static long time;

            /// <summary>
            ///     Reference to node manager actor
            /// </summary>
            private readonly IActorRef nodeManager;

            /// <summary>
            ///     Initializes a new instance of the <see cref="VirtualNode" /> class.
            /// </summary>
            /// <param name="num">
            ///     The node number.
            /// </param>
            /// <param name="nodeManager">
            ///     The node manager.
            /// </param>
            /// <param name="system">
            ///     The system.
            /// </param>
            /// <param name="releaseId">
            ///     The installed release id
            /// </param>
            /// <param name="template">
            ///     The installed template
            /// </param>
            public VirtualNode(
                int num,
                IActorRef nodeManager,
                ActorSystem system,
                int? releaseId = null,
                string template = null)
            {
                this.nodeManager = nodeManager;
                this.Number = num;
                this.NodeId = Guid.NewGuid();
                this.Address = new UniqueAddress(new Address("akka.tcp", "ClusterKit", $"testNode{num}", num), num);
                this.Description = new NodeDescription
                                       {
                                           ContainerType = "test",
                                           NodeAddress = this.Address.Address,
                                           NodeId = this.NodeId,
                                           NodeTemplate = template ?? "t1",
                                           StartTimeStamp = GetNextTime(),
                                           ReleaseId = releaseId ?? 1
                                       };

                this.Client = system.ActorOf(Props.Create(() => new VirtualClient(this)), $"virtualNode{num}");
            }

            /// <summary>
            ///     Gets the node address
            /// </summary>
            public UniqueAddress Address { get; }

            /// <summary>
            ///     Gets the virtual node client actor
            /// </summary>
            public IActorRef Client { get; }

            /// <summary>
            ///     Gets the node description
            /// </summary>
            public NodeDescription Description { get; }

            /// <summary>
            /// Gets a value indicating whether the node was started
            /// </summary>
            public bool IsUp { get; private set; }

            /// <summary>
            ///     Gets the node uid
            /// </summary>
            public Guid NodeId { get; }

            /// <summary>
            ///     Gets the node number
            /// </summary>
            public int Number { get; }

            /// <summary>
            ///     Gets the next timestamp (1 second later)
            /// </summary>
            /// <returns>The timestamp</returns>
            public static long GetNextTime()
            {
                return Interlocked.Add(ref time, 1L);
            }

            /// <summary>
            ///     Commands virtual node to emulate shutdown process
            /// </summary>
            public void GoDown()
            {
                if (!this.IsUp)
                {
                    return;
                }

                this.nodeManager.Tell(
                    new ClusterEvent.MemberRemoved(
                        ClusterExtensions.MemberCreate(
                            this.Address,
                            1,
                            MemberStatus.Removed,
                            ImmutableHashSet<string>.Empty),
                        MemberStatus.Up));
                this.IsUp = false;
            }

            /// <summary>
            ///     Commands virtual node to emulate start process
            /// </summary>
            public void GoUp()
            {
                if (this.IsUp)
                {
                    return;
                }

                this.nodeManager.Tell(
                    new ClusterEvent.MemberUp(
                        ClusterExtensions.MemberCreate(
                            this.Address,
                            1,
                            MemberStatus.Up,
                            ImmutableHashSet<string>.Empty)));
                this.IsUp = true;
            }

            /// <summary>
            ///     Virtual client actor
            /// </summary>
            public class VirtualClient : ReceiveActor
            {
                /// <summary>
                ///     Initializes a new instance of the <see cref="VirtualClient" /> class.
                /// </summary>
                /// <param name="node">
                ///     The virtual node.
                /// </param>
                public VirtualClient(VirtualNode node)
                {
                    this.Receive<TestMessage<Identify>>(
                        m => this.Sender.Tell(new ActorIdentity(m.Message.MessageId, this.Self)));
                    this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(node.Description, this.Self));
                    this.Receive<TestMessage<ShutdownMessage>>(m => node.GoDown());
                }

                /// <inheritdoc />
                protected override bool AroundReceive(Receive receive, object message)
                {
                    Context.GetLogger().Warning($"Got message of type {message.GetType().FullName}");
                    return base.AroundReceive(receive, message);
                }

                /// <inheritdoc />
                protected override void Unhandled(object message)
                {
                    Context.GetLogger().Error($"Got unhandled message of type {message.GetType().FullName}");
                    base.Unhandled(message);
                }
            }
        }
    }
}