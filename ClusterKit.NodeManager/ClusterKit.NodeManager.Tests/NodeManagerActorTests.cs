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
    using ClusterKit.Core.Data;
    using ClusterKit.Core.Data.TestKit;
    using ClusterKit.Core.EF;
    using ClusterKit.Core.EF.TestKit;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.Core.TestKit;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
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

            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();

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

            var virtualNodes = new List<VirtualNode>
                                   {
                                       new VirtualNode(1, testActor, this.Sys),
                                       new VirtualNode(2, testActor, this.Sys),
                                       new VirtualNode(3, testActor, this.Sys),
                                   };

            virtualNodes.ForEach(
                n =>
                    {
                        router.RegisterVirtualNode(n.Address.Address, n.Client);
                        n.GoUp();
                    });

            var descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(3, descriptions.Count);
            Assert.True(descriptions.All(d => !d.IsObsolete));

            packageFactory.Storage["TestModule-1"].Version = "0.2.0";

            testActor.Tell(new ReloadPackageListRequest());
            descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));

            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(2, virtualNodes.Count(n => n.IsUp));
            Assert.True(descriptions.All(d => d.IsObsolete));

            // this will check that no additional upgrades will cause new nodes to reboot
            testActor.Tell(new ReloadPackageListRequest());
            descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));

            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(2, virtualNodes.Count(n => n.IsUp));
            Assert.True(descriptions.All(d => d.IsObsolete));

            var virtualNode = virtualNodes.First(n => n.Number == 1);
            Assert.False(virtualNode.IsUp); // the oldest node gone to upgrade

            virtualNode.Description.StartTimeStamp = VirtualNode.GetNextTime();
            virtualNode.Description.Modules[0].Version = "0.2.0";
            virtualNode.GoUp();

            descriptions = await testActor.Ask<List<NodeDescription>>(new ActiveNodeDescriptionsRequest(), TimeSpan.FromSeconds(1));

            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.Equal(2, virtualNodes.Count(n => n.IsUp));
            Assert.Equal(1, descriptions.Count(d => d.IsObsolete));
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
            testActor.Tell(new RestActionMessage<NodeTemplate, int> { ActionType = EnActionType.Update, Data = nodeTemplate });

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
        /// Tests template distibution
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task TemplateDistributionTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
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
                        MininmumRequiredInstances = 1,
                        MaximumNeededInstances = 2
                    });

            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template2",
                        Code = "test-template2",
                        Id = 2,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 0,
                        MaximumNeededInstances = null
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);

            var templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(1, templates.Count);
            Assert.Equal("test-template", templates.First().Code); // there is less then minimum required of test-template

            var node1 = new VirtualNode(1, testActor, this.Sys)
            {
                Description =
                                    {
                                        NodeTemplate = "test-template",
                                        ContainerType = "test",
                                        Modules = new List<PackageDescription> {new PackageDescription { Id = "TestModule-1", Version = "0.0.0" } }
                                    }
            };

            router.RegisterVirtualNode(node1.Address.Address, node1.Client);
            node1.GoUp();

            stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);
            Assert.Equal(1, stats.Templates.First(t => t.Name == "test-template").ActiveNodes);
            Assert.Equal(1, stats.Templates.First(t => t.Name == "test-template").ObsoleteNodes);

            templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var node2 = new VirtualNode(2, testActor, this.Sys)
            {
                Description =
                                    {
                                        NodeTemplate = "test-template",
                                        ContainerType = "test",
                                        Modules = new List<PackageDescription> {new PackageDescription { Id = "TestModule-1", Version = "0.1.0" } }
                                    }
            };
            router.RegisterVirtualNode(node2.Address.Address, node2.Client);
            node2.GoUp();

            stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);
            Assert.Equal(1, stats.Templates.First(t => t.Name == "test-template").ActiveNodes);
            Assert.Equal(0, stats.Templates.First(t => t.Name == "test-template").ObsoleteNodes);
            Assert.Equal(1, stats.Templates.First(t => t.Name == "test-template").UpgradingNodes);
            Assert.False(node1.IsUp);

            templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            node1.Description.Modules[0].Version = "0.1.0";
            node1.GoUp();

            stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);
            Assert.Equal(2, stats.Templates.First(t => t.Name == "test-template").ActiveNodes);
            Assert.Equal(0, stats.Templates.First(t => t.Name == "test-template").ObsoleteNodes);
            Assert.Equal(0, stats.Templates.First(t => t.Name == "test-template").UpgradingNodes);

            templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(1, templates.Count);
        }

        /// <summary>
        /// Tests template distibution
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task TemplateDistributionWithUpgradeTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
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
                        MininmumRequiredInstances = 1,
                        MaximumNeededInstances = 2
                    });

            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template2",
                        Code = "test-template2",
                        Id = 2,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 0,
                        MaximumNeededInstances = null
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);

            var templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(1, templates.Count);
            Assert.Equal("test-template", templates.First().Code); // there is less then minimum required of test-template

            var node1 = new VirtualNode(1, testActor, this.Sys)
            {
                Description =
                                    {
                                        NodeTemplate = "test-template",
                                        ContainerType = "test",
                                        Modules = new List<PackageDescription> {new PackageDescription { Id = "TestModule-1", Version = "0.1.0" } }
                                    }
            };

            router.RegisterVirtualNode(node1.Address.Address, node1.Client);
            node1.GoUp();

            stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);
            Assert.Equal(1, stats.Templates.First(t => t.Name == "test-template").ActiveNodes);
            Assert.Equal(0, stats.Templates.First(t => t.Name == "test-template").ObsoleteNodes);

            templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var node2 = new VirtualNode(2, testActor, this.Sys)
            {
                Description =
                                    {
                                        NodeTemplate = "test-template",
                                        ContainerType = "test",
                                        Modules = new List<PackageDescription> {new PackageDescription { Id = "TestModule-1", Version = "0.1.0" } }
                                    }
            };
            router.RegisterVirtualNode(node2.Address.Address, node2.Client);
            node2.GoUp();

            stats = await testActor.Ask<TemplatesUsageStatistics>(new TemplatesStatisticsRequest());
            Assert.NotNull(stats);
            Assert.Equal(2, stats.Templates.Count);
            Assert.Equal(2, stats.Templates.First(t => t.Name == "test-template").ActiveNodes);
            Assert.Equal(0, stats.Templates.First(t => t.Name == "test-template").ObsoleteNodes);

            templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(1, templates.Count);
        }

        /// <summary>
        /// Tests template selection
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Fact]
        public async Task TemplateSelectionTest()
        {
            var templatesFactory =
                (UniversalTestDataFactory<ConfigurationContext, NodeTemplate, int>)
                this.WindsorContainer.Resolve<DataFactory<ConfigurationContext, NodeTemplate, int>>();
            var packageFactory =
                (UniversalTestDataFactory<string, PackageDescription, string>)
                this.WindsorContainer.Resolve<DataFactory<string, PackageDescription, string>>();

            var router = (TestMessageRouter)this.WindsorContainer.Resolve<IMessageRouter>();

            await packageFactory.Insert(new PackageDescription { Id = "TestModule-1", Version = "0.1.0" });
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
                        MininmumRequiredInstances = 0,
                        MaximumNeededInstances = null,
                        Priority = 1000000
                    });

            await
                templatesFactory.Insert(
                    new NodeTemplate
                    {
                        Name = "test-template2",
                        Code = "test-template2",
                        Id = 2,
                        Version = 0,
                        ContainerTypes = new List<string> { "test" },
                        Packages = new List<string> { "TestModule-1" },
                        MininmumRequiredInstances = 0,
                        MaximumNeededInstances = null,
                        Priority = 1
                    });

            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var templates = await testActor.Ask<List<NodeTemplate>>(new AvailableTemplatesRequest { ContainerType = "test" });
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var description =
                await
                testActor.Ask<NodeStartUpConfiguration>(
                    new NewNodeTemplateRequest { ContainerType = "test", NodeUid = Guid.NewGuid() });
            Assert.NotNull(description);
            Assert.Equal("test-template", description.NodeTemplate); // we have 1 in million chance of false failure
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
                container.Register(Component.For<IMessageRouter>().ImplementedBy<TestMessageRouter>().LifestyleSingleton());
            }
        }

        /// <summary>
        /// Virtual node to emulate cluster node responses to update
        /// </summary>
        private class VirtualNode
        {
            private static long time;

            /// <summary>
            /// Reference to node manager actor
            /// </summary>
            private readonly IActorRef nodeManager;

            public VirtualNode(int num, IActorRef nodeManager, ActorSystem system)
            {
                this.nodeManager = nodeManager;
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
                    StartTimeStamp = GetNextTime(),
                    NodeTemplateVersion = 0
                };

                this.Client = system.ActorOf(Props.Create(() => new VirtualClient(this)), $"virtualNode{num}");
            }

            public UniqueAddress Address { get; }

            public IActorRef Client { get; }

            public NodeDescription Description { get; }

            /// <summary>
            /// Gets the node state
            /// </summary>
            public bool IsUp { get; private set; }

            public Guid NodeId { get; }

            public int Number { get; }

            public static long GetNextTime()
            {
                return Interlocked.Add(ref time, 1L);
            }

            public void GoDown()
            {
                if (!this.IsUp)
                {
                    return;
                }

                this.nodeManager.Tell(
                    new ClusterEvent.MemberRemoved(
                        Member.Create(this.Address, 1, MemberStatus.Removed, ImmutableHashSet<string>.Empty), MemberStatus.Up));
                this.IsUp = false;
            }

            public void GoUp()
            {
                if (this.IsUp)
                {
                    return;
                }

                this.nodeManager.Tell(
                    new ClusterEvent.MemberUp(
                        Member.Create(this.Address, 1, MemberStatus.Up, ImmutableHashSet<string>.Empty)));
                this.IsUp = true;
            }

            public class VirtualClient : ReceiveActor
            {
                private VirtualNode node;

                public VirtualClient(VirtualNode node)
                {
                    this.node = node;

                    this.Receive<TestMessage<Identify>>(m => this.Sender.Tell(new ActorIdentity(m.Message.MessageId, this.Self)));
                    this.Receive<NodeDescriptionRequest>(m => this.Sender.Tell(this.node.Description, this.Self));
                    this.Receive<TestMessage<ShutdownMessage>>(m => this.node.GoDown());
                }

                protected override bool AroundReceive(Receive receive, object message)
                {
                    Context.GetLogger().Warning($"Got message of type {message.GetType().FullName}");
                    return base.AroundReceive(receive, message);
                }

                protected override void Unhandled(object message)
                {
                    Context.GetLogger().Error($"Got unhandled message of type {message.GetType().FullName}");
                    base.Unhandled(message);
                }
            }
        }
    }
}