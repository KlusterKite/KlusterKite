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
    using KlusterKite.NodeManager.Migrator;
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
        /// The migration state test.
        /// </summary>
        /// <param name="resources">
        /// The array of string representing resources.
        /// Each string contains 3 chars.
        /// 1. Dependency type: B for CodeDependsOnResource, A for ResourceDependsOnCode 
        /// 2. The migration direction: U for upgrade, D for downgrade, S for stay and B from Broken (when source and destination migration does not contain one another)
        /// 3. The resource position: S - source, D - destination, M - no source nor destination (but can be migrated), B - broken, C - create, O - obsolete
        /// </param>
        /// <param name="configurationPosition">The current active configuration (1 or 2)</param>
        /// <param name="nodesPosition">The nodes current configuration (1 or 2)</param>
        /// <param name="expectedStartBitState">
        /// The expected start bit state. The list of bites to check at start
        /// 1. OperationIsInProgress
        /// 2. CanCancelMigration
        /// 3. CanCreateMigration
        /// 4. CanFinishMigration
        /// 5. CanMigrateResources
        /// 6. CanUpdateNodesToDestination
        /// 7. CanUpdateNodesToSource
        /// </param>
        /// <param name="expectedStartStep">
        /// The expected start step.
        /// </param>
        /// <param name="expectedSteps">
        /// The expected states.
        /// </param>
        /// <param name="expectedMigratableResources">
        /// The list of expected migratable resources (the index numbers of resources in the resources list).
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [Theory]
        [InlineData(new[] { "BDS" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BDS" }, 2, 1, "0000001", EnMigrationSteps.NodesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BDS" }, 1, 2, "0000010", EnMigrationSteps.NodesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BDS" }, 2, 2, "0000101", EnMigrationSteps.NodesUpdated, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BDD" }, 2, 2, "0001100", EnMigrationSteps.Finish, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BDD", "BDS" }, 2, 2, "0000100", EnMigrationSteps.PostNodesResourcesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 1 })]

        [InlineData(new[] { "ADS" }, 1, 1, "0100100", EnMigrationSteps.Start, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "ADD" }, 1, 1, "0000110", EnMigrationSteps.PreNodeResourcesUpdated, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "ADD", "ADS" }, 1, 1, "0000100", EnMigrationSteps.PreNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0, 1 })]
        [InlineData(new[] { "ADD" }, 2, 1, "0000001", EnMigrationSteps.NodesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "ADD" }, 1, 2, "0000010", EnMigrationSteps.NodesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "ADD" }, 2, 2, "0001001", EnMigrationSteps.Finish, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new int[0])]

        [InlineData(new[] { "AUS" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "AUS" }, 2, 1, "0000001", EnMigrationSteps.NodesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "AUS" }, 1, 2, "0000010", EnMigrationSteps.NodesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "AUS" }, 2, 2, "0000101", EnMigrationSteps.NodesUpdated, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "AUD" }, 2, 2, "0001100", EnMigrationSteps.Finish, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "AUD", "AUS" }, 2, 2, "0000100", EnMigrationSteps.PostNodesResourcesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 1 })]

        [InlineData(new[] { "BUS" }, 1, 1, "0100100", EnMigrationSteps.Start, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BUD" }, 1, 1, "0000110", EnMigrationSteps.PreNodeResourcesUpdated, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BUD", "BUS" }, 1, 1, "0000100", EnMigrationSteps.PreNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0, 1 })]
        [InlineData(new[] { "BUD" }, 2, 1, "0000001", EnMigrationSteps.NodesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BUD" }, 2, 2, "0001001", EnMigrationSteps.Finish, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new int[0])]

        [InlineData(new[] { "BUS", "BDS", "AUS", "ADS" }, 1, 1, "0100100", EnMigrationSteps.Start, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 3 })]
        [InlineData(new[] { "BUD", "BDS", "AUS", "ADS" }, 1, 1, "0000100", EnMigrationSteps.PreNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 3 })]
        [InlineData(new[] { "BUD", "BDS", "AUS", "ADD" }, 1, 1, "0000110", EnMigrationSteps.PreNodeResourcesUpdated, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 3 })]
        [InlineData(new[] { "BUD", "BDS", "AUS", "ADD" }, 2, 1, "0000001", EnMigrationSteps.NodesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BUD", "BDS", "AUS", "ADD" }, 1, 2, "0000010", EnMigrationSteps.NodesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BUD", "BDS", "AUS", "ADD" }, 2, 2, "0000101", EnMigrationSteps.NodesUpdated, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 1, 2 })]
        [InlineData(new[] { "BUD", "BDD", "AUS", "ADD" }, 2, 2, "0000100", EnMigrationSteps.PostNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 1, 2 })]
        [InlineData(new[] { "BUD", "BDD", "AUD", "ADD" }, 2, 2, "0001100", EnMigrationSteps.Finish, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 1, 2 })]

        [InlineData(new[] { "BDO" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "ADO" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "AUO" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BUO" }, 1, 1, "0100010", EnMigrationSteps.Start, "Start, NodesUpdating, Finish", new int[0])]

        [InlineData(new[] { "BDC" }, 1, 1, "0100100", EnMigrationSteps.Start, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "ADC" }, 2, 2, "0000101", EnMigrationSteps.NodesUpdated, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "AUC" }, 2, 2, "0000101", EnMigrationSteps.NodesUpdated, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BUC" }, 1, 1, "0100100", EnMigrationSteps.Start, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]

        [InlineData(new[] { "BDM" }, 2, 2, "0000100", EnMigrationSteps.PostNodesResourcesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "ADM" }, 1, 1, "0000100", EnMigrationSteps.PreNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "AUM" }, 2, 2, "0000100", EnMigrationSteps.PostNodesResourcesUpdating, "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0 })]
        [InlineData(new[] { "BUM" }, 1, 1, "0000100", EnMigrationSteps.PreNodesResourcesUpdating, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish", new[] { 0 })]

        [InlineData(new[] { "BDB" }, 1, 1, "0000000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "ADB" }, 2, 2, "0000000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "AUB" }, 2, 2, "0000000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BUB" }, 1, 1, "0000000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]

        [InlineData(new[] { "BUS", "BDD", "AUD", "ADS" }, 2, 2, "0000101", EnMigrationSteps.Recovery, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 3, 1, 2 })]
        [InlineData(new[] { "BUS", "BDD", "AUD", "ADS" }, 1, 1, "0000110", EnMigrationSteps.Recovery, "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish", new[] { 0, 3, 1, 2 })]

        [InlineData(new[] { "BBS" }, 1, 1, "0100000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]
        [InlineData(new[] { "BBM" }, 1, 1, "0000000", EnMigrationSteps.Broken, "Start, NodesUpdating, Finish", new int[0])]

        public async Task MigrationStateTest(
            string[] resources,
            int configurationPosition,
            int nodesPosition,
            string expectedStartBitState,
            EnMigrationSteps expectedStartStep,
            string expectedSteps,
            int[] expectedMigratableResources)
        {
            using (var ds = this.GetContext())
            {
                if (configurationPosition == 2)
                {
                    var sourceConfiguration = ds.Configurations.First(c => c.Id == 1);
                    var destinationConfiguration = ds.Configurations.First(c => c.Id == 2);

                    sourceConfiguration.State = EnConfigurationState.Archived;
                    destinationConfiguration.State = EnConfigurationState.Active;
                }

                var migration = new Migration
                                    {
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
            var actor = this.CreateActor(nodesPosition, out node1, out node2, out node3);
            var migratorStates = resources.Select(
                r =>
                    {
                        var dependency = r[0] == 'B'
                                             ? EnResourceDependencyType.CodeDependsOnResource
                                             : EnResourceDependencyType.ResourceDependsOnCode;
                        string[] sourcePoints;
                        string[] destinationPoints;
                        ResourceDescription resourceDescription;

                        switch (r[1])
                        {
                            case 'U':
                                sourcePoints = new[] { "first" };
                                destinationPoints = new[] { "first", "second", "third" };
                                break;
                            case 'D':
                                sourcePoints = new[] { "first", "second", "third" };
                                destinationPoints = new[] { "first" };
                                break;
                            case 'S':
                                sourcePoints = new[] { "first", "second", "third" };
                                destinationPoints = new[] { "first", "second", "third" };
                                break;
                            case 'B':
                                sourcePoints = new[] { "first", "second", "third" };
                                destinationPoints = new[] { "first", "second", "forth" };
                                break;
                            default: throw new ArgumentException(nameof(resources));
                        }

                        switch (r[2])
                        {
                            case 'S':
                                resourceDescription = new ResourceDescription(sourcePoints.Last());
                                break;
                            case 'D':
                                resourceDescription = new ResourceDescription(destinationPoints.Last());
                                break;
                            case 'M':
                                resourceDescription = new ResourceDescription("second");
                                break;
                            case 'B':
                                resourceDescription = new ResourceDescription("non-existent");
                                break;
                            case 'C':
                                resourceDescription =
                                    new ResourceDescription(null) { Position = EnResourcePosition.Destination };
                                break;
                            case 'O':
                                resourceDescription =
                                    new ResourceDescription(destinationPoints.Last())
                                        {
                                            Position = EnResourcePosition
                                                .Source
                                        };
                                break;
                            default: throw new ArgumentException(nameof(resources));
                        }

                        var resourceDescriptions = new[] { resourceDescription };
                        return new MigratorDescription(
                                   sourcePoints,
                                   destinationPoints,
                                   resourceDescriptions)
                                   {
                                       DependencyType = dependency
                                   };
                    }).ToArray();

            var migrationState = this.CreateMigrationActorMigrationState(migratorStates);
            actor.Tell(migrationState);
            this.ExpectNoMsg();
            var state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.NotNull(state.MigrationState);
            Assert.Equal(expectedStartStep, state.CurrentMigrationStep);
            Assert.Equal(expectedSteps, string.Join(", ", state.MigrationSteps));
            Assert.Equal(expectedStartBitState, GetCanBitString(state));

            var migratableResources = state.MigrationState.MigratableResources
                .Select(
                    mr => migratorStates.FirstOrDefault(
                        ms => ms.TypeName == mr.MigratorTypeName && ms.Resources[0].Code == mr.Code))
                .Select(ms => Array.IndexOf(migratorStates, ms)).ToArray();
            Assert.Equal(
                string.Join(", ", expectedMigratableResources.Select(i => i.ToString())),
                string.Join(", ", migratableResources.Select(i => i.ToString())));
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
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Archived, sourceConfiguration.State);
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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
            var migratorDescriptions = new MigratorDescription(
                new[] { "first", "second" },
                new[] { "first" },
                new[] { new ResourceDescription("first") });
            var migrationState = this.CreateMigrationActorMigrationState(
                migratorDescriptions);

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = migratorDescriptions.TypeName,
                                         ResourceCode = migratorDescriptions.Resources[0].Code,
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

            Assert.Equal(EnMigrationSteps.PreNodesResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
            var migratorDescriptions = new MigratorDescription(
                new[] { "first", "second" },
                new[] { "first" },
                new[] { new ResourceDescription("second") });
            var migrationState = this.CreateMigrationActorMigrationState(
                migratorDescriptions);

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = migratorDescriptions.TypeName,
                                         ResourceCode = migratorDescriptions.Resources[0].Code,
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

            Assert.Equal(EnMigrationSteps.PreNodesResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
            Assert.Equal(
                "Start, NodesUpdating, NodesUpdated, PostNodesResourcesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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
                Assert.Equal(EnConfigurationState.Archived, sourceConfiguration.State);
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("first") }));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("second") }));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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

            Assert.Equal(EnMigrationSteps.PreNodeResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("second") }));

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

            Assert.Equal(EnMigrationSteps.PreNodeResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            using (var ds = this.GetContext())
            {
                var sourceConfiguration = ds.Configurations.First(r => r.Id == 1);
                var destinationConfiguration = ds.Configurations.First(r => r.Id == 2);
                Assert.Equal(EnConfigurationState.Archived, sourceConfiguration.State);
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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
            var migratorDescriptions = new MigratorDescription(
                new[] { "first" },
                new[] { "first", "second" },
                new[] { new ResourceDescription("second") });
            var migrationState = this.CreateMigrationActorMigrationState(
                migratorDescriptions);

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

            Assert.Equal(EnMigrationSteps.PreNodeResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = migratorDescriptions.TypeName,
                                         ResourceCode = migratorDescriptions.Resources[0].Code,
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

            Assert.Equal(EnMigrationSteps.PreNodesResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("first") }));

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
            var migratorDescriptions = new MigratorDescription(
                new[] { "first" },
                new[] { "first", "second" },
                new[] { new ResourceDescription("first") });
            var migrationState = this.CreateMigrationActorMigrationState(
                migratorDescriptions);

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
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = migratorDescriptions.TypeName,
                                         ResourceCode = migratorDescriptions.Resources[0].Code,
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

            Assert.Equal(EnMigrationSteps.PreNodesResourcesUpdating, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));

            migrationState = this.CreateMigrationActorMigrationState(
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("second") }));

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

            Assert.Equal(EnMigrationSteps.PreNodeResourcesUpdated, state.CurrentMigrationStep);
            Assert.Equal(
                "Start, PreNodesResourcesUpdating, PreNodeResourcesUpdated, NodesUpdating, Finish",
                string.Join(", ", state.MigrationSteps));
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
        /// Checking the resource states, resource is broken and should be updated manually
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateRecheckTest()
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

            Assert.True(await actor.Ask<bool>(new RecheckState(), TimeSpan.FromMilliseconds(500)));
            this.ExpectMsg<RecheckState>("/user/migrationActor");

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("second", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.True(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.NotNull(state.ConfigurationState.MigratableResources);
            Assert.Equal(0, state.ConfigurationState.MigratableResources.Count);
        }

        /// <summary>
        /// Checking the resource states, resource is not in last migration point and should be updated manually
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateResourceUpgradeTest()
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

            Assert.NotNull(state.ConfigurationState.MigratableResources);
            Assert.Equal(1, state.ConfigurationState.MigratableResources.Count);
            var resource = state.ConfigurationState.MigratableResources[0];

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = resource.MigratorTypeName,
                                         ResourceCode = resource.Code,
                                         TemplateCode = resource.TemplateCode
                                     }
                             };

            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("second", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.True(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.NotNull(state.ConfigurationState.MigratableResources);
            Assert.Equal(0, state.ConfigurationState.MigratableResources.Count);
        }

        /// <summary>
        /// Checking the resource states, resource is not in last migration point and should be updated manually
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateResourceCreateTest()
        {
            var actor = this.CreateActor(1);
            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState(null, new[] { "first" });
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

            Assert.NotNull(state.ConfigurationState.MigratableResources);
            Assert.Equal(1, state.ConfigurationState.MigratableResources.Count);
            var resource = state.ConfigurationState.MigratableResources[0];

            var update = new List<ResourceUpgrade>
                             {
                                 new ResourceUpgrade
                                     {
                                         MigratorTypeName = resource.MigratorTypeName,
                                         ResourceCode = resource.Code,
                                         TemplateCode = resource.TemplateCode
                                     }
                             };

            Assert.True(await actor.Ask<bool>(update, TimeSpan.FromSeconds(1)));
            this.ExpectMsg<List<ResourceUpgrade>>("/user/migrationActor");
            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromSeconds(10));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.True(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.False(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("second", new[] { "first", "second" });
            actor.Tell(migrationActorConfigurationState);
            this.ExpectNoMsg();

            state = await actor.Ask<ResourceState>(new ResourceStateRequest(), TimeSpan.FromMilliseconds(500));
            Assert.Null(state.MigrationState);
            Assert.NotNull(state.ConfigurationState);
            Assert.False(state.OperationIsInProgress);
            Assert.False(state.CanCancelMigration);
            Assert.True(state.CanCreateMigration);
            Assert.False(state.CanFinishMigration);
            Assert.False(state.CanMigrateResources);
            Assert.False(state.CanUpdateNodesToDestination);
            Assert.False(state.CanUpdateNodesToSource);

            Assert.NotNull(state.ConfigurationState.MigratableResources);
            Assert.Equal(0, state.ConfigurationState.MigratableResources.Count);
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

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first" });
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("first") }),
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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
                new MigratorDescription(
                    new[] { "first", "second" },
                    new[] { "first" },
                    new[] { new ResourceDescription("second") }));

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

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first" });
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
        }

        /// <summary>
        /// Checking migration creation with unchanged resources
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateMigrationCreateResourcesUnchangedTest()
        {
            var actor = this.CreateActor(1);

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first" });
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first" },
                    new[] { new ResourceDescription("first") }));

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

            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first" });
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
                new MigratorDescription(
                    new[] { "first" },
                    new[] { "first", "second" },
                    new[] { new ResourceDescription("first") }));

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
        }

        /// <summary>
        /// Checking the resource states in normal state
        /// </summary>
        /// <returns>The resource states</returns>
        [Fact]
        public async Task ResourceConfigurationStateOkTest()
        {
            var actor = this.CreateActor(1);
            var migrationActorConfigurationState =
                this.CreateMigrationActorConfigurationState("first", new[] { "first" });
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
                                        FrameworkRuntimeType = ConfigurationCheckTestsBase
                                            .Net46
                                    },
                                TimeSpan.FromSeconds(1));
            Assert.NotNull(templates);
            Assert.Equal(2, templates.Count);

            var description = await testActor.Ask<NodeStartUpConfiguration>(
                                  new NewNodeTemplateRequest
                                      {
                                          ContainerType = "test",
                                          FrameworkRuntimeType =
                                              ConfigurationCheckTestsBase.Net46,
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
        /// Creates a bit string from state "Can" properties
        /// </summary>
        /// <param name="state">The migrator state</param>
        /// <returns>The bit string</returns>
        private static string GetCanBitString(ResourceState state)
        {
            return (state.OperationIsInProgress ? "1" : "0")
                   + (state.CanCancelMigration ? "1" : "0")
                   + (state.CanCreateMigration ? "1" : "0")
                   + (state.CanFinishMigration ? "1" : "0")
                   + (state.CanMigrateResources ? "1" : "0")
                   + (state.CanUpdateNodesToDestination ? "1" : "0")
                   + (state.CanUpdateNodesToSource ? "1" : "0");
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
        /// <param name="migratorDescriptions">The test migrators description</param>
        /// <returns>
        /// the migration actor state
        /// </returns>
        private MigrationActorMigrationState CreateMigrationActorMigrationState(
            params MigratorDescription[] migratorDescriptions)
        {
            var migratorTemplate = new MigratorTemplate { Code = "test" };
            var sourceMigratorStates = migratorDescriptions.Where(d => d.SourcePoints != null).Select(
                d =>
                    {
                        var resources = d.Resources.Where(r => r.Position != EnResourcePosition.Destination)
                            .Select(
                                r => new ResourceConfigurationState
                                         {
                                             Code = r.Code,
                                             CurrentPoint = r.CurrentPoint,
                                             Name = r.Code,
                                             MigratorTypeName = d.TypeName,
                                             TemplateCode = migratorTemplate.Code
                                         }).ToList();
                        return new MigratorConfigurationState
                                   {
                                       DependencyType = d.DependencyType,
                                       LastDefinedPoint = d.SourcePoints.Last(),
                                       MigrationPoints = d.SourcePoints.ToList(),
                                       Priority = d.Priority,
                                       TypeName = d.TypeName,
                                       Name = d.TypeName,
                                       Resources = resources
                                   };
                    }).ToList();
            var destinationMigratorStates = migratorDescriptions.Where(d => d.DestinationPoints != null).Select(
                d =>
                    {
                        var resources = d.Resources.Where(r => r.Position != EnResourcePosition.Source)
                            .Select(
                                r => new ResourceConfigurationState
                                         {
                                             Code = r.Code,
                                             CurrentPoint = r.CurrentPoint,
                                             Name = r.Code,
                                             MigratorTypeName = d.TypeName,
                                             TemplateCode = migratorTemplate.Code
                                }).ToList();
                        return new MigratorConfigurationState
                                   {
                                       DependencyType = d.DependencyType,
                                       LastDefinedPoint = d.DestinationPoints.Last(),
                                       MigrationPoints = d.DestinationPoints.ToList(),
                                       Priority = d.Priority,
                                       TypeName = d.TypeName,
                                       Name = d.TypeName,
                                       Resources = resources
                                   };
                    }).ToList();
            var sourceState =
                new MigratorTemplateConfigurationState
                    {
                        Code = migratorTemplate.Code,
                        Template = migratorTemplate,
                        MigratorsStates = sourceMigratorStates
                    };
            var destinationState =
                new MigratorTemplateConfigurationState
                    {
                        Code = migratorTemplate.Code,
                        Template = migratorTemplate,
                        MigratorsStates = destinationMigratorStates
                    };

            return new MigrationActorMigrationState
                       {
                           TemplateStates = MigrationActor.CreateMigrationState(
                               new[] { sourceState },
                               new[] { destinationState }).ToList()
                       };
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
                                                  LastDefinedPoint =
                                                      configurationPoints.Last(),
                                                  Name = "test migrator",
                                                  MigrationPoints =
                                                      configurationPoints.ToList(),
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
        /// The migrator description
        /// </summary>
        private class MigratorDescription
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MigratorDescription"/> class.
            /// </summary>
            /// <param name="sourcePoints">
            /// The source points.
            /// </param>
            /// <param name="destinationPoints">
            /// The destination points.
            /// </param>
            /// <param name="resources">
            /// The resources.
            /// </param>
            public MigratorDescription(
                string[] sourcePoints,
                string[] destinationPoints,
                ResourceDescription[] resources)
            {
                this.SourcePoints = sourcePoints;
                this.DestinationPoints = destinationPoints;
                this.Resources = resources;
            }

            /// <summary>
            /// Gets the resources
            /// </summary>
            public ResourceDescription[] Resources { get; }

            /// <summary>
            /// Gets the list of source points
            /// </summary>
            public string[] SourcePoints { get; }

            /// <summary>
            /// Gets the list of destination points
            /// </summary>
            public string[] DestinationPoints { get; }

            /// <summary>
            /// Gets or sets the migrator priority
            /// </summary>
            public decimal Priority { get; set; } = 1M;

            /// <summary>
            /// Gets or sets the type of resource dependency
            /// </summary>
            public EnResourceDependencyType DependencyType { get; set; } =
                EnResourceDependencyType.CodeDependsOnResource;

            /// <summary>
            /// Gets the migrator type name
            /// </summary>
            public string TypeName { get; } = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// The test resource description
        /// </summary>
        private class ResourceDescription
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceDescription"/> class.
            /// </summary>
            /// <param name="currentPoint">
            /// The current point.
            /// </param>
            public ResourceDescription(string currentPoint)
            {
                this.CurrentPoint = currentPoint;
            }

            /// <summary>
            /// Gets the resource current state
            /// </summary>
            public string CurrentPoint { get; }

            /// <summary>
            /// Gets or sets the resource id
            /// </summary>
            public string Code { get; set; } = Guid.NewGuid().ToString();

            /// <summary>
            /// Gets or sets the test resource position
            /// </summary>
            public EnResourcePosition Position { get; set; } = EnResourcePosition.SourceAndDestination;
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

                container.RegisterType<ConfigurationDataFactory>()
                    .As<DataFactory<ConfigurationContext, Configuration, int>>();
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
                                                     ConfigurationCheckTestsBase.Net46,
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
                                                     ConfigurationCheckTestsBase.Net46,
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
                                                     ConfigurationCheckTestsBase.Net46,
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
                                                      ConfigurationCheckTestsBase.Net46,
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
                                                      ConfigurationCheckTestsBase.Net46,
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
                                                      ConfigurationCheckTestsBase.Net46,
                                                      "dp3 2.0.0")
                                          }
                              };
                var dp12 = new TestPackage("dp1", "2.0.0");

                var dp22 = new TestPackage("dp2", "2.0.0");

                var dp32 = new TestPackage("dp3", "2.0.0");

                return new TestRepository(p1, p2, p3, dp1, dp2, dp3, p12, p22, p32, dp12, dp22, dp32);
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
            private readonly ActorSystem system;

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