// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerActorTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing node manager actor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Configuration;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.TestKit;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.Core;
    using KlusterKite.Core.Ping;
    using KlusterKite.Core.TestKit;
    using KlusterKite.Data;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.EF;
    using KlusterKite.Data.EF.InMemory;
    using KlusterKite.NodeManager.Client.Messages;
    using KlusterKite.NodeManager.Client.Messages.Migration;
    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.NodeManager.Tests.Mock;

    using Xunit;
    using Xunit.Abstractions;

    using Installer = KlusterKite.Core.TestKit.Installer;

    /// <summary>
    ///     Testing node manager actor
    /// </summary>
    [Collection("KlusterKite.NodeManager.Tests.ConfigurationContext")]
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
            // ReSharper disable once StringLiteralTypo
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            var response = await testActor.Ask<PongMessage>(new PingMessage(), TimeSpan.FromSeconds(10));
            Assert.NotNull(response);
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
            // ReSharper disable once StringLiteralTypo
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var nodeAddress = new UniqueAddress(new Address("akka.tcp", "KlusterKite", "testNode1", 1), 1);

            this.SetAutoPilot(
                new DelegateAutoPilot(
                    delegate(IActorRef sender, object message)
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
                                                        new
                                                            PackageDescription
                                                                {
                                                                    Id
                                                                        = "TestModule",
                                                                    Version
                                                                        = "0.1.0"
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
            var router = (TestMessageRouter)this.Container.Resolve<IMessageRouter>();

            // ReSharper disable once StringLiteralTypo
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            this.ExpectNoMsg();
            var activeNode = new VirtualNode(1, testActor, this.Sys, 1);
            var obsoleteNode = new VirtualNode(2, testActor, this.Sys, 2);
            router.RegisterVirtualNode(activeNode.Address.Address, activeNode.Client);
            router.RegisterVirtualNode(obsoleteNode.Address.Address, obsoleteNode.Client);
            activeNode.GoUp();
            obsoleteNode.GoUp();
            this.ExpectNoMsg(TimeSpan.FromSeconds(1));

            var descriptions = await testActor.Ask<List<NodeDescription>>(
                                   new ActiveNodeDescriptionsRequest(),
                                   TimeSpan.FromSeconds(1));
            Assert.NotNull(descriptions);
            Assert.Equal(2, descriptions.Count);
            Assert.False(descriptions.FirstOrDefault(d => d.NodeAddress == activeNode.Address.Address)?.IsObsolete);
            Assert.True(descriptions.FirstOrDefault(d => d.NodeAddress == obsoleteNode.Address.Address)?.IsObsolete);
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
            int newConfigurationId;
            using (var context = this.GetContext())
            {
                var oldConfiguration = context.Configurations.First(r => r.State == EnConfigurationState.Active);
                oldConfiguration.State = EnConfigurationState.Obsolete;

                var newConfiguration = ConfigurationCheckTestsBase.CreateConfiguration();
                newConfiguration.State = EnConfigurationState.Active;
                newConfiguration.Settings.SeedAddresses = new List<string>();
                newConfiguration.Settings.NugetFeed = "http://nuget/";
                newConfiguration.Settings.NodeTemplates[0].MinimumRequiredInstances = 2;
                newConfiguration.Settings.NodeTemplates[0].PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            [ConfigurationCheckTestsBase.Net46] =
                            new List<PackageDescription>(
                                newConfiguration.Settings.Packages)
                        };

                newConfiguration.CompatibleTemplatesBackward =
                    new List<CompatibleTemplate>
                        {
                            new CompatibleTemplate
                                {
                                    CompatibleConfigurationId = 1,
                                    TemplateCode = "t1"
                                }
                        };

                context.Configurations.Add(newConfiguration);
                context.SaveChanges();
                newConfigurationId = newConfiguration.Id;
            }

            var router = (TestMessageRouter)this.Container.Resolve<IMessageRouter>();

            // ReSharper disable once StringLiteralTypo
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            this.ExpectNoMsg();
            var activeNode = new VirtualNode(1, testActor, this.Sys, 1);
            var obsoleteNode = new VirtualNode(2, testActor, this.Sys, newConfigurationId);
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
            Assert.False(descriptions.FirstOrDefault(d => d.NodeAddress == activeNode.Address.Address)?.IsObsolete);
            Assert.False(descriptions.FirstOrDefault(d => d.NodeAddress == obsoleteNode.Address.Address)?.IsObsolete);
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeCancel()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            var actor = this.CreateActor(1);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationCancel(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeFinish()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            var actor = this.CreateActor(2);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationFinish(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeNodesRollback()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(2, out node1, out node2, out node3);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(new NodesUpgrade { Target = EnMigrationSide.Source }, TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Active, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Obsolete, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(1, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeNodesUpdate()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(1, out node1, out node2, out node3);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(
                    new NodesUpgrade { Target = EnMigrationSide.Destination },
                    TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Faulted, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Active, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(2, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeResourceRollBack()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            var actor = this.CreateActor(2);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = "TestMigrator",
                                         ResourceCode = "0",
                                         Target = EnMigrationSide.Source,
                                         TemplateCode = "test"
                                     }
                             };
            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateDowngradeResourceUpdate()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Downgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            var actor = this.CreateActor(2);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = "TestMigrator",
                                         ResourceCode = "0",
                                         Target = EnMigrationSide.Destination,
                                         TemplateCode = "test"
                                     }
                             };
            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));
            
            migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, NodesUpdated, ResourcesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateStayCancel()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Stay,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            var actor = this.CreateActor(1);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationCancel(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateStayFinish()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Stay,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            var actor = this.CreateActor(2);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationFinish(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateStayNodesRollback()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Stay,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(2, out node1, out node2, out node3);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(new NodesUpgrade { Target = EnMigrationSide.Source }, TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Active, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Faulted, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(1, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateStayNodesUpdate()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Stay,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(1, out node1, out node2, out node3);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(
                    new NodesUpgrade { Target = EnMigrationSide.Destination },
                    TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Obsolete, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Active, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(2, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeCancel()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            var actor = this.CreateActor(1);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationCancel(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeFinish()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            var actor = this.CreateActor(2);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(await actor.Ask<bool>(new MigrationFinish(), TimeSpan.FromSeconds(1)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            Assert.Null(this.GetActiveMigrationFromDatabase());
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeNodesRollback()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                sourceConfiguration.State = EnConfigurationState.Obsolete;
                destinationConfiguration.State = EnConfigurationState.Active;
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(2, out node1, out node2, out node3);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(new NodesUpgrade { Target = EnMigrationSide.Source }, TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Active, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Faulted, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(1, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);
            
            Assert.Equal(EnMigrationSteps.ResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeNodesUpdate()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            var actor = this.CreateActor(1, out node1, out node2, out node3);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            Assert.True(
                await actor.Ask<bool>(
                    new NodesUpgrade { Target = EnMigrationSide.Destination },
                    TimeSpan.FromSeconds(1)));
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.NodesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Obsolete, sourceConfiguration.State);
                Assert.Equal(EnConfigurationState.Active, destinationConfiguration.State);
            }

            this.CheckNodesUpgrade(2, node1, node2, node3);

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.True(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.True(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Finish, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeResourceRollBack()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            var actor = this.CreateActor(1);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = "TestMigrator",
                                         ResourceCode = "0",
                                         Target = EnMigrationSide.Source,
                                         TemplateCode = "test"
                                     }
                             };
            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// The actor is in migration state, updating resources
        /// </summary>
        /// <returns>The async task</returns>
        [Fact]
        public async Task ResourceMigrationStateUpgradeResourceUpdate()
        {
            using (var ds = this.GetContext())
            {
                var migration = new Migration
                                    {
                                        Direction = EnMigrationDirection.Upgrade,
                                        FromConfigurationId = 1,
                                        ToConfigurationId = 2,
                                        IsActive = true,
                                        Started = DateTimeOffset.Now,
                                        State = EnMigrationState.Ready
                                    };
                ds.Migrations.Add(migration);
                ds.SaveChanges();
            }

            var actor = this.CreateActor(1);
            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.Start, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = "TestMigrator",
                                         ResourceCode = "0",
                                         Target = EnMigrationSide.Destination,
                                         TemplateCode = "test"
                                     }
                             };
            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.Equal(EnMigrationSteps.ResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal("Start, ResourcesUpdating, ResourcesUpdated, NodesUpdating, Finish", string.Join(", ", state.MigrationSteps));
        }

        /// <summary>
        /// Checking the resource states, resource is broken and should be updated manually
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateBrokenResourceTest()
        {
            var actor = this.CreateActor(1);
            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("third", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);
        }

        /// <summary>
        /// Checking the resource states in initialization state
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateInitializationTest()
        {
            var actor = this.CreateActor(1);
            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);
        }

        /// <summary>
        /// Checking the resource states
        /// </summary>
        /// <returns>The resource states</returns>
        /// <remarks>
        /// Migration that updates one resources and downgrades other are not supported
        /// </remarks>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateDirectionConflictTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState = this.CreateMigrationActorConfigurationState("first", new[] { "first" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var migration =
                (await actor.Ask<CrudActionResponse<Migration>>(
                     new UpdateClusterRequest { Id = 2 },
                     TimeSpan.FromSeconds(1))).Data;

            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            var migrationId = migration.Id;

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetActiveMigrationFromDatabase();
            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first", "second" },
                new[] { new[] { "first" }, new[] { "first", "second" } },
                new[] { new[] { "first", "second" }, new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetMigrationFromDatabase(migrationId);
            Assert.NotNull(migration);
            Assert.True(migration.IsActive);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            Assert.Null(migration.Direction);
        }

        /// <summary>
        /// Checking the migration downgrade creation
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateDowngradeTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("second", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var migration =
                (await actor.Ask<CrudActionResponse<Migration>>(
                     new UpdateClusterRequest { Id = 2 },
                     TimeSpan.FromSeconds(1))).Data;

            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetActiveMigrationFromDatabase();
            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            var migrationId = migration.Id;

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "second" },
                new[] { new[] { "first", "second" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetMigrationFromDatabase(migrationId);
            Assert.NotNull(migration);
            Assert.True(migration.IsActive);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Ready, migration.State);
            Assert.Equal(EnMigrationDirection.Downgrade, migration.Direction);
        }

        /// <summary>
        /// Checking the resource states
        /// </summary>
        /// <returns>The resource states</returns>
        /// <remarks>
        /// Migration that updates one resources and downgrades other are not supported
        /// </remarks>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateFailedTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState = this.CreateMigrationActorConfigurationState("first", new[] { "first" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var migration =
                (await actor.Ask<CrudActionResponse<Migration>>(
                     new UpdateClusterRequest { Id = 2 },
                     TimeSpan.FromSeconds(1))).Data;

            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();
            var migrationId = migration.Id;

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetActiveMigrationFromDatabase();
            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);

            actor.Tell(new MigrationActorInitializationFailed { Errors = new List<MigrationLogRecord>() });
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetMigrationFromDatabase(migrationId);
            Assert.NotNull(migration);
            Assert.True(migration.IsActive);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            Assert.Null(migration.Direction);
        }

        /// <summary>
        /// Checking migration creation with unchanged resources
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateResourcesUnchangedTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState = this.CreateMigrationActorConfigurationState("first", new[] { "first" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var migration =
                (await actor.Ask<CrudActionResponse<Migration>>(
                     new UpdateClusterRequest { Id = 2 },
                     TimeSpan.FromSeconds(1))).Data;

            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetActiveMigrationFromDatabase();
            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            var migrationId = migration.Id;

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.True(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetMigrationFromDatabase(migrationId);
            Assert.NotNull(migration);
            Assert.True(migration.IsActive);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Ready, migration.State);
        }

        /// <summary>
        /// Checking the migration upgrade creation
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateUpgradeTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState = this.CreateMigrationActorConfigurationState("first", new[] { "first" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var migration =
                (await actor.Ask<CrudActionResponse<Migration>>(
                     new UpdateClusterRequest { Id = 2 },
                     TimeSpan.FromSeconds(1))).Data;

            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            this.ExpectMsg<RecheckState>("/user/migrationActor");
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetActiveMigrationFromDatabase();
            Assert.NotNull(migration);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Preparing, migration.State);
            var migrationId = migration.Id;

            var migrationState = this.CreateMigrationActorMigrationState(
                new[] { "first" },
                new[] { new[] { "first" } },
                new[] { new[] { "first", "second" } });

            actor.Tell(migrationState);
            this.ExpectNoMsg();
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Null(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.True(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migration = this.GetMigrationFromDatabase(migrationId);
            Assert.NotNull(migration);
            Assert.True(migration.IsActive);
            Assert.Equal(1, migration.FromConfigurationId);
            Assert.Equal(2, migration.ToConfigurationId);
            Assert.Equal(EnMigrationState.Ready, migration.State);
            Assert.Equal(EnMigrationDirection.Upgrade, migration.Direction);
        }

        /// <summary>
        /// Checking the resource states in normal state
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateOkTest()
        {
            var actor = this.CreateActor(1);
            var migrationActorConfigurationState = this.CreateMigrationActorConfigurationState("first", new[] { "first" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();
            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.True(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            var migration = this.GetActiveMigrationFromDatabase();
            Assert.Null(migration);
        }

        /// <summary>
        /// Checking the resource states, cluster node should be migrated
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateUnmigratedNodeTest()
        {
            using (var context = this.GetContext())
            {
                var configuration = context.Configurations.First(r => r.State == EnConfigurationState.Active);
                configuration.Settings.NodeTemplates.First().MinimumRequiredInstances = 100;
                context.SaveChanges();
            }

            var actor = this.CreateActor(0);

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("second", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);
        }

        /// <summary>
        /// Checking the resource states, resource should be migrated
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateUnmigratedResourceTest()
        {
            var actor = this.CreateActor(1);
            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.True(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);
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
                var configuration = context.Configurations.First(r => r.State == EnConfigurationState.Active);

                var template1 = ConfigurationCheckTestsBase.CreateConfiguration().Settings.NodeTemplates.First();
                template1.Code = "t1";
                template1.Priority = 1;
                template1.PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            {
                                ConfigurationCheckTestsBase.Net46,
                                new List<PackageDescription>()
                            }
                        };

                var template2 = ConfigurationCheckTestsBase.CreateConfiguration().Settings.NodeTemplates.First();
                template2.Code = "t2";
                template2.Priority = 1000000;
                template2.PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            {
                                ConfigurationCheckTestsBase.Net46,
                                new List<PackageDescription>()
                            }
                        };

                configuration.Settings.NodeTemplates = new[] { template1, template2 }.ToList();
                context.SaveChanges();
            }

            // ReSharper disable once StringLiteralTypo
            var testActor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");

            var templates = await testActor.Ask<List<NodeTemplate>>(
                                new AvailableTemplatesRequest
                                    {
                                        ContainerType = "test",
                                        FrameworkRuntimeType = ConfigurationCheckTestsBase.Net46
                                    },
                                TimeSpan.FromSeconds(1));
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var description = await testActor.Ask<NodeStartUpConfiguration>(
                                  new NewNodeTemplateRequest
                                      {
                                          ContainerType = "test",
                                          FrameworkRuntimeType = ConfigurationCheckTestsBase.Net46,
                                          NodeUid = Guid.NewGuid()
                                      },
                                  TimeSpan.FromSeconds(1));
            Assert.NotNull(description);
            Assert.Equal("t2", description.NodeTemplate); // we have 1 in million chance of false failure
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            this.GetContext().Database.EnsureDeleted();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Check the node upgrade process
        /// </summary>
        /// <param name="newConfigurationId">The new node configurations</param>
        /// <param name="node1">The node 1</param>
        /// <param name="node2">The node 2</param>
        /// <param name="node3">The node 3</param>
        private void CheckNodesUpgrade(int newConfigurationId, VirtualNode node1, VirtualNode node2, VirtualNode node3)
        {
            Assert.False(node1.IsUp);
            Assert.True(node2.IsUp);
            Assert.True(node3.IsUp);
            node1.Description.StartTimeStamp = VirtualNode.GetNextTime();
            node1.Description.ConfigurationId = newConfigurationId;
            node1.GoUp();
            this.ExpectNoMsg();
            Assert.True(node1.IsUp);
            Assert.False(node2.IsUp);
            Assert.True(node3.IsUp);
            node2.Description.StartTimeStamp = VirtualNode.GetNextTime();
            node2.Description.ConfigurationId = newConfigurationId;
            node2.GoUp();
            this.ExpectNoMsg();
            Assert.True(node1.IsUp);
            Assert.True(node2.IsUp);
            Assert.False(node3.IsUp);
            node3.Description.StartTimeStamp = VirtualNode.GetNextTime();
            node3.Description.ConfigurationId = newConfigurationId;
            node3.GoUp();
            this.ExpectNoMsg();
        }

        /// <summary>
        /// Initializes the <see cref="NodeManagerActor"/>
        /// </summary>
        /// <param name="configurationId">
        /// The node configuration Id.
        /// </param>
        /// <returns>
        /// The reference to the actor
        /// </returns>
        private IActorRef CreateActor(int configurationId)
        {
            VirtualNode node1;
            VirtualNode node2;
            VirtualNode node3;
            return this.CreateActor(configurationId, out node1, out node2, out node3);
        }

        /// <summary>
        /// Initializes the <see cref="NodeManagerActor"/>
        /// </summary>
        /// <param name="configurationId">
        /// The node configuration Id.
        /// </param>
        /// <param name="node1">
        /// The node 1.
        /// </param>
        /// <param name="node2">
        /// The node 2.
        /// </param>
        /// <param name="node3">
        /// The node 3.
        /// </param>
        /// <returns>
        /// The reference to the actor
        /// </returns>
        private IActorRef CreateActor(
            int configurationId,
            out VirtualNode node1,
            out VirtualNode node2,
            out VirtualNode node3)
        {
            var router = (TestMessageRouter)this.Container.Resolve<IMessageRouter>();
            this.ActorOf(() => new TestActorForwarder(this.TestActor), "migrationActor");

            // ReSharper disable once StringLiteralTypo
            var actor = this.ActorOf(this.Sys.DI().Props<NodeManagerActor>(), "nodemanager");
            node1 = new VirtualNode(1, actor, this.Sys, configurationId);
            router.RegisterVirtualNode(node1.Address.Address, node1.Client);
            node1.GoUp();
            node2 = new VirtualNode(2, actor, this.Sys, configurationId);
            router.RegisterVirtualNode(node2.Address.Address, node2.Client);
            node2.GoUp();
            node3 = new VirtualNode(3, actor, this.Sys, configurationId);
            router.RegisterVirtualNode(node3.Address.Address, node3.Client);
            node3.GoUp();
            this.ExpectNoMsg();
            return actor;
        }

        /// <summary>
        /// Creates the migration actor state with active migration
        /// </summary>
        /// <param name="resourcePoints">
        /// The list of migration point for resources
        /// </param>
        /// <param name="sourcePoints">
        /// The list of defined migration points for resources in source configuration
        /// </param>
        /// <param name="destinationPoints">
        /// The list of defined migration points for resources in destination configuration
        /// </param>
        /// <returns>
        /// the migration actor state
        /// </returns>
        private MigrationActorMigrationState CreateMigrationActorMigrationState(
            string[] resourcePoints,
            string[][] sourcePoints,
            string[][] destinationPoints)
        {
            Assert.Equal(resourcePoints.Length, sourcePoints.Length);
            Assert.Equal(resourcePoints.Length, destinationPoints.Length);

            var migratorTemplate = new MigratorTemplate { Code = "test" };
            var resourceMigrationStates = new List<ResourceMigrationState>();

            var direction = EnMigrationDirection.Stay;

            for (var i = 0; i < resourcePoints.Length; i++)
            {
                var migrationToSourceExecutor =
                    destinationPoints[i].Contains(resourcePoints[i])
                    && destinationPoints[i].Contains(sourcePoints[i].Last())
                        ? EnMigrationSide.Destination
                        : sourcePoints[i].Contains(resourcePoints[i])
                            ? EnMigrationSide.Source
                            : (EnMigrationSide?)null;

                var migrationToDestinationExecutor =
                    sourcePoints[i].Contains(resourcePoints[i]) && sourcePoints[i].Contains(destinationPoints[i].Last())
                        ? EnMigrationSide.Source
                        : destinationPoints[i].Contains(resourcePoints[i])
                            ? EnMigrationSide.Destination
                            : (EnMigrationSide?)null;

                var resourceMigrationState =
                    new ResourceMigrationState
                        {
                            Code = i.ToString(),
                            CurrentPoint = resourcePoints[i],
                            SourcePoint = sourcePoints[i].Last(),
                            DestinationPoint = destinationPoints[i].Last(),
                            Name = i.ToString(),
                            MigrationToSourceExecutor = migrationToSourceExecutor,
                            MigrationToDestinationExecutor = migrationToDestinationExecutor,
                        };
                resourceMigrationStates.Add(resourceMigrationState);

                if (sourcePoints[i].Contains(resourcePoints[i]) && destinationPoints[i].Contains(resourcePoints[i])
                    && sourcePoints[i].Last() == destinationPoints[i].Last())
                {
                }
                else if (sourcePoints[i].Contains(resourcePoints[i])
                         && sourcePoints[i].Contains(destinationPoints[i].Last()))
                {
                    switch (direction)
                    {
                        case EnMigrationDirection.Stay:
                            direction = EnMigrationDirection.Downgrade;
                            break;
                        case EnMigrationDirection.Upgrade:
                            direction = EnMigrationDirection.Undefined;
                            break;
                    }
                }
                else if (destinationPoints[i].Contains(resourcePoints[i])
                         && destinationPoints[i].Contains(sourcePoints[i].Last()))
                {
                    switch (direction)
                    {
                        case EnMigrationDirection.Stay:
                            direction = EnMigrationDirection.Upgrade;
                            break;
                        case EnMigrationDirection.Downgrade:
                            direction = EnMigrationDirection.Undefined;
                            break;
                    }
                }
                else
                {
                    direction = EnMigrationDirection.Undefined;
                }
            }

            var migratorMigrationState =
                new MigratorMigrationState
                    {
                        Direction = direction,
                        Name = "test migrator",
                        TypeName = "TestMigrator",
                        Position = EnMigratorPosition.Merged,
                        Resources = resourceMigrationStates
                    };

            var migratorMigrationStates = new[] { migratorMigrationState }.ToList();
            var migratorTemplateMigrationState =
                new MigratorTemplateMigrationState
                    {
                        Code = "test",
                        DestinationTemplate = migratorTemplate,
                        SourceTemplate = migratorTemplate,
                        Position = EnMigratorPosition.Merged,
                        Migrators = migratorMigrationStates
                    };

            var migratorTemplateMigrationStates = new[] { migratorTemplateMigrationState }.ToList();

            var hasDestinationResources = migratorTemplateMigrationStates.Any(
                t => t.Migrators.Any(m => m.Resources.Any(r => r.Position == EnResourcePosition.Destination)));
            var hasSourceResources = migratorTemplateMigrationStates.Any(
                t => t.Migrators.Any(m => m.Resources.Any(r => r.Position == EnResourcePosition.Source)));
            var hasBrokenResources = migratorTemplateMigrationStates.Any(
                t => t.Migrators.Any(m => m.Resources.Any(r => r.Position == EnResourcePosition.Undefined)));

            var position = hasBrokenResources || (hasDestinationResources && hasSourceResources)
                               ? EnMigrationActorMigrationPosition.Broken
                               : hasSourceResources
                                   ? EnMigrationActorMigrationPosition.Source
                                   : hasDestinationResources
                                       ? EnMigrationActorMigrationPosition.Destination
                                       : EnMigrationActorMigrationPosition.NoMigrationNeeded;

            var migrationState =
                new MigrationActorMigrationState
                    {
                        Position = position,
                        TemplateStates = migratorTemplateMigrationStates
                    };
            return migrationState;
        }

        /// <summary>
        /// Creates the migration actor state without active migration
        /// </summary>
        /// <param name="resourcePoint">The resource current state</param>
        /// <param name="configurationPoints">The configuration defined points</param>
        /// <returns>the migration actor state</returns>
        private MigrationActorConfigurationState CreateMigrationActorConfigurationState(
            string resourcePoint,
            string[] configurationPoints)
        {
            var resources = new[]
                                {
                                    new ResourceConfigurationState
                                        {
                                            Code = "test",
                                            CurrentPoint = resourcePoint,
                                            Name = "test"
                                        }
                                }.ToList();
            var migratorsStates = new[]
                                      {
                                          new MigratorConfigurationState
                                              {
                                                  LastDefinedPoint = configurationPoints.Last(),
                                                  Name = "test migrator",
                                                  MigrationPoints = configurationPoints.ToList(),
                                                  TypeName = "TestMigrator",
                                                  Resources = resources
                                              }
                                      }.ToList();

            var migratorTemplate = new MigratorTemplate { Code = "test" };
            var templateStates = new[]
                                     {
                                         new MigratorTemplateConfigurationState
                                             {
                                                 Code = "test",
                                                 Template = migratorTemplate,
                                                 MigratorsStates = migratorsStates
                                             }
                                     }.ToList();

            var migrationActorConfigurationState = new MigrationActorConfigurationState { States = templateStates };
            return migrationActorConfigurationState;
        }

        /// <summary>
        /// Gets the active migration in database
        /// </summary>
        /// <returns>The active migration</returns>
        private Migration GetActiveMigrationFromDatabase()
        {
            using (var ds = this.GetContext())
            {
                return ds.Migrations.FirstOrDefault(m => m.IsActive);
            }
        }

        /// <summary>
        ///     Creates database context
        /// </summary>
        /// <returns>The database context</returns>
        private ConfigurationContext GetContext()
        {
            var connectionString = this.Container.Resolve<Config>()
                .GetString(NodeManagerActor.ConfigConnectionStringPath);
            var databaseName = this.Container.Resolve<Config>().GetString(NodeManagerActor.ConfigDatabaseNamePath);
            return this.Container.Resolve<UniversalContextFactory>()
                .CreateContext<ConfigurationContext>("InMemory", connectionString, databaseName);
        }

        /// <summary>
        /// Gets the active migration in database
        /// </summary>
        /// <param name="migrationId">
        /// The migration Id.
        /// </param>
        /// <returns>
        /// The active migration
        /// </returns>
        private Migration GetMigrationFromDatabase(int migrationId)
        {
            using (var ds = this.GetContext())
            {
                return ds.Migrations.FirstOrDefault(m => m.Id == migrationId);
            }
        }

        /// <summary>
        ///     Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

            /// <summary>
            ///     Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers =
                    new List<BaseInstaller>
                        {
                            new Installer(),
                            new TestInstaller(),
                            new Data.Installer(),
                            new Data.EF.Installer(),
                            new Data.EF.InMemory.Installer(),
                        };
                return pluginInstallers;
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
                KlusterKite.NodeManager.ConfigurationDatabaseName = ""{Guid.NewGuid():N}""
                KlusterKite.NodeManager.ConfigurationDatabaseProviderName = ""InMemory""
                KlusterKite.NodeManager.ConfigurationDatabaseConnectionString = """"
                KlusterKite.NodeManager.MigrationActorSubstitute = ""/user/migrationActor""

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
            protected override void PostStart(IComponentContext componentContext)
            {
                base.PostStart(componentContext);
                var contextManager = componentContext.Resolve<UniversalContextFactory>();
                var config = componentContext.Resolve<Config>();
                var connectionString = config.GetString(NodeManagerActor.ConfigConnectionStringPath);
                var databaseName = config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
                using (var context =
                    contextManager.CreateContext<ConfigurationContext>("InMemory", connectionString, databaseName))
                {
                    context.ResetValueGenerators();
                    context.Database.EnsureDeleted();

                    context.Configurations.Add(CreateConfiguration());
                    var configuration = CreateConfiguration();
                    configuration.State = EnConfigurationState.Ready;
                    context.Configurations.Add(configuration);
                    context.SaveChanges();
                    componentContext.Resolve<ActorSystem>().Log.Info(
                        "!!! Created configuration with id {ConfigurationId} in Database {ConnectionString}",
                        configuration.Id,
                        connectionString);
                }
            }

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder container, Config config)
            {
                container.RegisterAssemblyTypes(typeof(NodeManagerActor).GetTypeInfo().Assembly)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));
                container.RegisterAssemblyTypes(typeof(Core.Installer).GetTypeInfo().Assembly)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));

                container.RegisterType<ConfigurationDataFactory>().As<DataFactory<ConfigurationContext, Configuration, int>>();
                var packageRepository = this.CreateTestRepository();
                container.RegisterInstance(packageRepository).As<IPackageRepository>();
                container.RegisterType<TestMessageRouter>().As<IMessageRouter>().SingleInstance();
            }

            /// <summary>
            /// Creates a new configuration object
            /// </summary>
            /// <returns>The configuration object</returns>
            private static Configuration CreateConfiguration()
            {
                var configuration = ConfigurationCheckTestsBase.CreateConfiguration();
                configuration.State = EnConfigurationState.Active;
                configuration.Settings.SeedAddresses = new List<string>();
                configuration.Settings.NugetFeed = "http://nuget/";
                configuration.Settings.NodeTemplates[0].MinimumRequiredInstances = 2;
                configuration.Settings.NodeTemplates[0].PackagesToInstall =
                    new Dictionary<string, List<PackageDescription>>
                        {
                            [ConfigurationCheckTestsBase.Net46] =
                            new List<PackageDescription>(
                                configuration.Settings.Packages)
                        };
                return configuration;
            }

            /// <summary>
            ///     Creates the test repository
            /// </summary>
            /// <returns>The test repository</returns>
            private IPackageRepository CreateTestRepository()
            {
                var p1 = new TestPackage("p1", "1.0.0")
                             {
                                 DependencySets =
                                     new[]
                                         {
                                             ConfigurationCheckTestsBase
                                                 .CreatePackageDependencySet(
                                                     ConfigurationCheckTestsBase
                                                         .Net46,
                                                     "dp1 1.0.0")
                                         }
                             };

                var p2 = new TestPackage("p2", "1.0.0")
                             {
                                 DependencySets =
                                     new[]
                                         {
                                             ConfigurationCheckTestsBase
                                                 .CreatePackageDependencySet(
                                                     ConfigurationCheckTestsBase
                                                         .Net46,
                                                     "dp2 1.0.0")
                                         }
                             };

                var p3 = new TestPackage("p3", "1.0.0")
                             {
                                 DependencySets =
                                     new[]
                                         {
                                             ConfigurationCheckTestsBase
                                                 .CreatePackageDependencySet(
                                                     ConfigurationCheckTestsBase
                                                         .Net46,
                                                     "dp3 2.0.0")
                                         }
                             };
                var dp1 = new TestPackage("dp1", "1.0.0");

                var dp2 = new TestPackage("dp2", "1.0.0");

                var dp3 = new TestPackage("dp3", "1.0.0");

                var p12 = new TestPackage("p1", "2.0.0")
                              {
                                  DependencySets =
                                      new[]
                                          {
                                              ConfigurationCheckTestsBase
                                                  .CreatePackageDependencySet(
                                                      ConfigurationCheckTestsBase
                                                          .Net46,
                                                      "dp1 2.0.0")
                                          }
                              };

                var p22 = new TestPackage("p2", "2.0.0")
                              {
                                  DependencySets =
                                      new[]
                                          {
                                              ConfigurationCheckTestsBase
                                                  .CreatePackageDependencySet(
                                                      ConfigurationCheckTestsBase
                                                          .Net46,
                                                      "dp2 2.0.0")
                                          }
                              };

                var p32 = new TestPackage("p3", "2.0.0")
                              {
                                  DependencySets =
                                      new[]
                                          {
                                              ConfigurationCheckTestsBase
                                                  .CreatePackageDependencySet(
                                                      ConfigurationCheckTestsBase
                                                          .Net46,
                                                      "dp3 2.0.0")
                                          }
                              };
                var dp12 = new TestPackage("dp1", "2.0.0");

                var dp22 = new TestPackage("dp2", "2.0.0");

                var dp32 = new TestPackage("dp3", "2.0.0");

                return new TestRepository(
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
            /// The actor system
            /// </summary>
            private ActorSystem system;

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
            /// <param name="configurationId">
            ///     The installed configuration id
            /// </param>
            /// <param name="template">
            ///     The installed template
            /// </param>
            public VirtualNode(
                int num,
                IActorRef nodeManager,
                ActorSystem system,
                int? configurationId = null,
                string template = null)
            {
                this.nodeManager = nodeManager;
                this.Number = num;
                this.NodeId = Guid.NewGuid();
                this.Address = new UniqueAddress(new Address("akka.tcp", "KlusterKite", $"testNode{num}", num), num);
                this.Description = new NodeDescription
                                       {
                                           ContainerType = "test",
                                           NodeAddress = this.Address.Address,
                                           NodeId = this.NodeId,
                                           NodeTemplate = template ?? "t1",
                                           StartTimeStamp = GetNextTime(),
                                           ConfigurationId = configurationId ?? 1
                                       };

                this.Client = system.ActorOf(Props.Create(() => new VirtualClient(this)), $"virtualNode{num}");
                this.system = system;
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
            /// Gets the node uid
            /// </summary>
            [UsedImplicitly]
            public Guid NodeId { get; }

            /// <summary>
            /// Gets the node number
            /// </summary>
            [UsedImplicitly]
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
            [UsedImplicitly]
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
                this.system.Log.Info("## Node {NodeNumber} is down", this.Number);
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
                this.system.Log.Info(
                    "## Node {NodeNumber} is up with configuration {ConfigurationId}",
                    this.Number,
                    this.Description.ConfigurationId);
            }

            /// <summary>
            ///     Virtual client actor
            /// </summary>
            [UsedImplicitly]
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
                    Context.GetLogger().Warning($"VirtualClient: Got message of type {message.GetType().FullName}");
                    return base.AroundReceive(receive, message);
                }

                /// <inheritdoc />
                protected override void Unhandled(object message)
                {
                    Context.GetLogger().Error(
                        $"VirtualClient: Got unhandled message of type {message.GetType().FullName}");
                    base.Unhandled(message);
                }
            }
        }
    }
}