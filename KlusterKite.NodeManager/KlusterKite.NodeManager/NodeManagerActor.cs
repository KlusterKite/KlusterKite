// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeManagerActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Singleton actor performing all node configuration related work
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.DI.Core;
    using Akka.Event;
    using Akka.Util.Internal;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.API.Client;
    using KlusterKite.Core;
    using KlusterKite.Core.Monads;
    using KlusterKite.Core.Ping;
    using KlusterKite.Core.Utils;
    using KlusterKite.Data;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.CRUD.Exceptions;
    using KlusterKite.Data.EF;
    using KlusterKite.NodeManager.Client.Messages;
    using KlusterKite.NodeManager.Client.Messages.Migration;
    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Messages;
    using KlusterKite.Security.Attributes;
    using KlusterKite.Security.Client;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Singleton actor performing all node configuration related work
    /// </summary>
    [UsedImplicitly]
    public class NodeManagerActor : BaseCrudActor<ConfigurationContext>, IWithUnboundedStash
    {
        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string ConfigConnectionStringPath = "KlusterKite.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string ConfigDatabaseNamePath = "KlusterKite.NodeManager.ConfigurationDatabaseName";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string ConfigDatabaseProviderNamePath = "KlusterKite.NodeManager.ConfigurationDatabaseProviderName";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string PackageRepositoryUrlPath = "KlusterKite.NodeManager.PackageRepository";

        /// <summary>
        /// List of node by templates
        /// </summary>
        private readonly Dictionary<string, List<Address>> activeNodesByTemplate =
            new Dictionary<string, List<Address>>();

        /// <summary>
        /// List of node by templates
        /// </summary>
        private readonly Dictionary<string, List<Guid>> awaitingRequestsByTemplate =
            new Dictionary<string, List<Guid>>();

        /// <summary>
        /// The data source context factory
        /// </summary>
        private readonly UniversalContextFactory contextFactory;

        /// <summary>
        /// In case of cluster is full, time span that new node candidate will repeat join request
        /// </summary>
        private readonly TimeSpan fullClusterWaitTimeout;

        /// <summary>
        /// After configuration request, it is assumed that new node of template should be up soon, and this is taken into account on subsequent configuration templates. This is timeout, after it it is supposed that something have gone wrong and that request is obsolete.
        /// </summary>
        private readonly TimeSpan newNodeJoinTimeout;

        /// <summary>
        /// Maximum number of <seealso cref="RequestDescriptionNotification"/> sent to newly joined node
        /// </summary>
        private readonly int newNodeRequestDescriptionNotificationMaxRequests;

        /// <summary>
        /// Timeout to send new <seealso cref="RequestDescriptionNotification"/> message to newly joined node
        /// </summary>
        private readonly TimeSpan newNodeRequestDescriptionNotificationTimeout;

        /// <summary>
        /// List of known node descriptions
        /// </summary>
        private readonly Dictionary<Address, NodeDescription> nodeDescriptions =
            new Dictionary<Address, NodeDescription>();

        /// <summary>  
        /// The nuget repository
        /// </summary>
        private readonly IPackageRepository nugetRepository;

        /// <summary>
        /// Random number generator
        /// </summary>
        private readonly Random random = new Random();

        /// <summary>
        /// List of pending node description requests
        /// </summary>
        private readonly Dictionary<Address, Cancelable> requestDescriptionNotifications =
            new Dictionary<Address, Cancelable>();

        /// <summary>
        /// the current resource state
        /// </summary>
        private readonly ResourceState resourceState = new ResourceState { OperationIsInProgress = true };

        /// <summary>
        /// Roles leaders
        /// </summary>
        private readonly Dictionary<string, Address> roleLeaders = new Dictionary<string, Address>();

        /// <summary>
        /// The node message router
        /// </summary>
        private readonly IMessageRouter router;

        /// <summary>
        /// Part of currently active nodes, that could be sent to upgrade at once. 1.0M - all active nodes (above minimum required) could be sent to upgrade.
        /// </summary>
        private readonly decimal upgradablePart;

        /// <summary>
        /// List of nodes with upgrade in progress
        /// </summary>
        private readonly Dictionary<Guid, UpgradeData> upgradingNodes = new Dictionary<Guid, UpgradeData>();

        /// <summary>
        /// The current cluster leader
        /// </summary>
        private Address clusterLeader;

        /// <summary>
        /// The database connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// The current active cluster migration
        /// </summary>
        private Migration currentMigration;

        /// <summary>
        /// The current active cluster configuration
        /// </summary>
        private Configuration currentConfiguration;

        /// <summary>
        /// The database name
        /// </summary>
        private string databaseName;

        /// <summary>
        /// The database provider name
        /// </summary>
        private string databaseProviderName;

        /// <summary>
        /// The resource migrator
        /// </summary>
        private ICanTell resourceMigrator;

        /// <summary>
        /// Handle to prevent excess upgrade messages
        /// </summary>
        private Cancelable upgradeMessageSchedule;

        /// <summary>
        /// Child actor workers
        /// </summary>
        private IActorRef workers;

        /// <summary>
        /// Child actor to update releases
        /// </summary>
        private IActorRef releaseManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActor"/> class.
        /// </summary>
        /// <param name="componentContext">
        /// The DI context
        /// </param>
        /// <param name="contextFactory">
        /// Configuration context factory
        /// </param>
        /// <param name="router">
        /// The node message router
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget repository
        /// </param>
        public NodeManagerActor(
            IComponentContext componentContext,
            UniversalContextFactory contextFactory,
            IMessageRouter router,
            IPackageRepository nugetRepository) : base(componentContext)
        {
            this.contextFactory = contextFactory;
            this.router = router;
            this.nugetRepository = nugetRepository;

            this.fullClusterWaitTimeout = Context.System.Settings.Config.GetTimeSpan(
                "KlusterKite.NodeManager.FullClusterWaitTimeout",
                TimeSpan.FromSeconds(60),
                false);
            this.newNodeJoinTimeout = Context.System.Settings.Config.GetTimeSpan(
                "KlusterKite.NodeManager.NewNodeJoinTimeout",
                TimeSpan.FromSeconds(30),
                false);
            this.newNodeRequestDescriptionNotificationTimeout = Context.System.Settings.Config.GetTimeSpan(
                "KlusterKite.NodeManager.NewNodeRequestDescriptionNotificationTimeout",
                TimeSpan.FromSeconds(10),
                false);
            this.newNodeRequestDescriptionNotificationMaxRequests = Context.System.Settings.Config.GetInt(
                "KlusterKite.NodeManager.NewNodeRequestDescriptionNotificationMaxRequests",
                10);
            this.upgradablePart = Context.System.Settings.Config.GetDecimal(
                "KlusterKite.NodeManager.NewNodeRequestDescriptionNotificationMaxRequests",
                10);

            this.Receive<InitializationMessage>(m => this.Initialize());
            this.Receive<object>(m => this.Stash.Stash());

            // ReSharper disable FormatStringProblem
            // ReSharper disable RedundantToStringCall
            Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());

            // ReSharper restore RedundantToStringCall
            // ReSharper restore FormatStringProblem
            this.Self.Tell(new InitializationMessage());
        }

        /// <summary>
        /// Gets or sets the stash. This will be automatically populated by the framework AFTER the constructor has been run.
        ///             Implement this as an auto property.
        /// </summary>
        /// <value>
        /// The stash.
        /// </value>
        public IStash Stash { get; set; }

        /// <summary>
        /// Method called before object modification in database
        /// </summary>
        /// <typeparam name="TObject">The type of object</typeparam>
        /// <param name="newObject">The new Object.</param>
        /// <param name="oldObject">The old Object.</param>
        /// <returns>
        /// The new version of object or null to prevent update
        /// </returns>
        protected override TObject BeforeUpdate<TObject>(TObject newObject, TObject oldObject)
        {
            if (typeof(TObject) == typeof(Configuration))
            {
                if (((Configuration)(object)oldObject).State != EnReleaseState.Draft)
                {
                    throw new Exception("Only draft releases can be updated");
                }
            }

            // ReSharper disable once RedundantTypeArgumentsOfMethod
            return base.BeforeUpdate<TObject>(newObject, oldObject);
        }

        /// <inheritdoc />
        protected override ConfigurationContext GetContext()
        {
            return this.contextFactory.CreateContext<ConfigurationContext>(
                this.databaseProviderName,
                this.connectionString,
                this.databaseName);
        }

        /// <summary>
        /// Is called when a message isn't handled by the current behavior of the actor
        ///             by default it fails with either a <see cref="T:Akka.Actor.DeathPactException"/> (in
        ///             case of an unhandled <see cref="T:Akka.Actor.Terminated"/> message) or publishes an <see cref="T:Akka.Event.UnhandledMessage"/>
        ///             to the actor's system's <see cref="T:Akka.Event.EventStream"/>
        /// </summary>
        /// <param name="message">The unhandled message.</param>
        protected override void Unhandled(object message)
        {
            if (message == null)
            {
                Context.GetLogger().Warning(
                    "{Type}: received null message from {ActorAddress}",
                    this.GetType().Name,
                    this.Sender.Path.ToString());
            }
            else
            {
                Context.GetLogger().Warning(
                    "{Type}: received unsupported message of type {MessageTypeName} from {ActorAddress}",
                    this.GetType().Name,
                    message.GetType().Name,
                    this.Sender.Path.ToString());
            }

            base.Unhandled(message);
        }

        /// <summary>
        /// Checks node for available upgrades
        /// </summary>
        /// <param name="nodeDescription">Node description to check</param>
        private void CheckNodeIsObsolete(NodeDescription nodeDescription)
        {
            nodeDescription.IsObsolete = false;
            if (string.IsNullOrWhiteSpace(nodeDescription.NodeTemplate))
            {
                return;
            }

            nodeDescription.IsObsolete = nodeDescription.ReleaseId != this.currentConfiguration.Id
                                         && this.currentConfiguration.CompatibleTemplatesBackward.All(
                                             ct => ct.TemplateCode != nodeDescription.NodeTemplate
                                                   || ct.CompatibleReleaseId != nodeDescription.ReleaseId);
        }

        /// <summary>
        /// Gets the active release from data source
        /// </summary>
        /// <param name="context">The data context</param>
        private void GetCurrentRelease(ConfigurationContext context)
        {
            var releases = context.Releases
                .Include(nameof(Configuration.CompatibleTemplatesBackward))
                .Include($"{nameof(Configuration.MigrationLogs)}")
                .Where(r => r.State == EnReleaseState.Active).ToList();
            this.currentConfiguration = releases.SingleOrDefault();

            if (this.currentConfiguration == null)
            {
                Context.GetLogger().Error(
                    "{Type}: Could not find any active release in database",
                    this.GetType().Name);
            }
        }

        /// <summary>
        /// Selects list of templates available for container
        /// </summary>
        /// <param name="containerType">The type of container</param>
        /// <param name="frameworkType">The type of framework on the container</param>
        /// <returns>The list of available templates</returns>
        private List<NodeTemplate> GetPossibleTemplatesForContainer(string containerType, string frameworkType)
        {
            var availableTemplates = this.currentConfiguration.Settings.NodeTemplates
                .Where(t => t.ContainerTypes.Contains(containerType) && t.PackagesToInstall.ContainsKey(frameworkType))
                .Select(
                    t => new
                             {
                                 Template = t,
                                 NodesCount =
                                 (this.activeNodesByTemplate.ContainsKey(t.Code)
                                      ? this.activeNodesByTemplate[t.Code].Count
                                      : 0) + (this.awaitingRequestsByTemplate.ContainsKey(t.Code)
                                                  ? this.awaitingRequestsByTemplate[t.Code].Count
                                                  : 0)
                             }).ToList();

            if (availableTemplates.Count == 0)
            {
                Context.GetLogger().Info(
                    "{Type}: There is no configuration available for {ContainerType} with framework {FrameworkName}",
                    this.GetType().Name,
                    containerType,
                    frameworkType);
                return new List<NodeTemplate>();
            }

            // first we choose among templates that have nodes less then minimum required
            var templates = availableTemplates
                .Where(
                    t => t.Template.MinimumRequiredInstances > 0 && t.NodesCount < t.Template.MinimumRequiredInstances)
                .Select(t => t.Template).ToList();

            if (templates.Count == 0)
            {
                // if all node templates has at least minimum required node quantity, we will use node template, until it has maximum needed quantity
                templates = availableTemplates
                    .Where(
                        t => !t.Template.MaximumNeededInstances.HasValue
                             || (t.Template.MaximumNeededInstances.Value > 0
                                 && t.NodesCount < t.Template.MaximumNeededInstances.Value)).Select(t => t.Template)
                    .ToList();
            }

            if (templates.Count == 0)
            {
                Context.GetLogger().Info(
                    "{Type}: Cluster is full, there is now room for {ContainerType} with framework {FrameworkName}",
                    this.GetType().Name,
                    containerType,
                    frameworkType);
            }

            return templates;
        }

        /// <summary>
        /// Checks current database connection. Updates database schema to latest version.
        /// </summary>
        private void InitDatabase()
        {
            this.connectionString = Context.System.Settings.Config.GetString(ConfigConnectionStringPath);
            this.databaseName = Context.System.Settings.Config.GetString(ConfigDatabaseNamePath);
            this.databaseProviderName = Context.System.Settings.Config.GetString(ConfigDatabaseProviderNamePath);

            using (var context = this.contextFactory.CreateContext<ConfigurationContext>(
                this.databaseProviderName,
                this.connectionString,
                this.databaseName))
            {
                this.GetCurrentRelease(context);
                this.currentMigration = context.Migrations.Include(nameof(Migration.FromConfiguration))
                    .Include(nameof(Migration.ToConfiguration))
                    .Include($"{nameof(Migration.Logs)}")
                    .FirstOrDefault(m => m.IsActive);
            }

            this.InitResourceState();
        }

        /// <summary>
        /// Supervisor initialization process
        /// </summary>
        private void Initialize()
        {
            try
            {
                this.InitDatabase();
            }
            catch (Exception e)
            {
                // ReSharper disable FormatStringProblem
                Context.GetLogger().Error(e, "{Type}: Exception during initialization", this.GetType().Name);

                // ReSharper restore FormatStringProblem
                Context.System.Scheduler.ScheduleTellOnce(
                    TimeSpan.FromSeconds(5),
                    this.Self,
                    new InitializationMessage(),
                    this.Self);
                return;
            }

            this.workers = Context.ActorOf(
                Props.Create(
                    () => new Worker(
                        this.databaseProviderName,
                        this.connectionString,
                        this.databaseName,
                        this.ComponentContext,
                        this.contextFactory,
                        this.Self)).WithRouter(this.Self.GetFromConfiguration(Context.System, "workers")),
                "workers");

            this.releaseManager = Context.ActorOf(
                Props.Create(
                    () => new ReleaseCheckActor(
                        this.ComponentContext,
                        this.contextFactory,
                        this.nugetRepository)),
                "release");

            var migrationActorSubstitute =
                Context.System.Settings.Config.GetString("KlusterKite.NodeManager.MigrationActorSubstitute");

            this.resourceMigrator = string.IsNullOrWhiteSpace(migrationActorSubstitute)
                                        ? (ICanTell)Context.ActorOf(
                                            Context.System.DI().Props<MigrationActor>(),
                                            "resourceMigrator")
                                        : Context.System.ActorSelection(migrationActorSubstitute);

            this.Become(this.Start);
            Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());
            this.Stash.UnstashAll();
        }

        /// <summary>
        /// Initializes migration with resource data
        /// </summary>
        private void InitMigration()
        {
            var hasUpgradingResources =
                this.resourceState.MigrationState.TemplateStates.Any(
                    t => t.Migrators.Any(m => m.Direction == EnMigrationDirection.Upgrade));
            var hasDowngradingResources =
                this.resourceState.MigrationState.TemplateStates.Any(
                    t => t.Migrators.Any(m => m.Direction == EnMigrationDirection.Downgrade));
            var hasBrokenResources =
                this.resourceState.MigrationState.TemplateStates.Any(
                    t => t.Migrators.Any(m => m.Direction == EnMigrationDirection.Undefined));

            if ((hasUpgradingResources && hasDowngradingResources) || hasBrokenResources)
            {
                this.resourceState.CanCancelMigration = true;
                this.resourceState.CanFinishMigration = false;
                this.resourceState.CanMigrateResources = false;
                this.resourceState.CanUpdateNodesToDestination = false;
                this.resourceState.CanUpdateNodesToSource = false;
                this.resourceState.MigrationSteps = new List<EnMigrationSteps> { EnMigrationSteps.Broken };
                this.resourceState.CurrentMigrationStep = EnMigrationSteps.Broken;
                return;
            }

            var direction = hasUpgradingResources
                                ? EnMigrationDirection.Upgrade
                                : hasDowngradingResources
                                    ? EnMigrationDirection.Downgrade
                                    : EnMigrationDirection.Stay;
            using (var ds = this.GetContext())
            {
                var migration = ds.Migrations.FirstOrDefault(m => m.IsActive);
                if (migration == null || migration.Id != this.currentMigration.Id)
                {
                    throw new Exception("Database synchronization failed for active migration");
                }

                migration.State = EnMigrationState.Ready;
                migration.Direction = direction;
                ds.SaveChanges();
            }

            this.InitDatabase();
        }

        /// <summary>
        /// Checks current resource state with active migration and without resource operation in progress
        /// </summary>
        private void InitResourceMigrationState()
        {
            this.resourceState.ReleaseState = null;
            this.resourceState.CanCreateMigration = false;

            if (this.resourceState.MigrationState == null)
            {
                this.resourceState.CanCancelMigration = this.currentMigration.State == EnMigrationState.Preparing;
                this.resourceState.CanFinishMigration = false;
                this.resourceState.CanMigrateResources = false;
                this.resourceState.CanUpdateNodesToDestination = false;
                this.resourceState.CanUpdateNodesToSource = false;
                this.resourceState.CurrentMigrationStep = null;
                this.resourceState.MigrationSteps = null;
                return;
            }

            if (this.currentMigration.State == EnMigrationState.Preparing)
            {
                this.InitMigration();
                return;
            }

            // checking for broken migration record
            if (!this.currentMigration.Direction.HasValue
                || this.currentMigration.Direction == EnMigrationDirection.Undefined)
            {
                Context.GetLogger().Error(
                    "{Type}: current migration is marked ready but has invalid direction value",
                    this.GetType().Name);
                this.resourceState.CanCancelMigration = false;
                this.resourceState.CanFinishMigration = false;
                this.resourceState.CanMigrateResources = false;
                this.resourceState.CanUpdateNodesToDestination = false;
                this.resourceState.CanUpdateNodesToSource = false;
                this.resourceState.MigrationSteps = new List<EnMigrationSteps> { EnMigrationSteps.Broken };
                this.resourceState.CurrentMigrationStep = EnMigrationSteps.Broken;
                return;
            }

            this.resourceState.MigrationSteps = this.GetMigrationSteps().ToList();

            this.resourceState.CanCancelMigration =
                (this.resourceState.MigrationState.Position == EnMigrationActorMigrationPosition.Source
                 || this.resourceState.MigrationState.Position == EnMigrationActorMigrationPosition.NoMigrationNeeded)
                && this.currentConfiguration.Id == this.currentMigration.FromConfigurationId
                && this.nodeDescriptions.Values.All(n => !n.IsObsolete);

            this.resourceState.CanFinishMigration =
                (this.resourceState.MigrationState.Position == EnMigrationActorMigrationPosition.Destination
                 || this.resourceState.MigrationState.Position == EnMigrationActorMigrationPosition.NoMigrationNeeded)
                && this.currentConfiguration.Id == this.currentMigration.ToConfigurationId
                && this.nodeDescriptions.Values.All(n => !n.IsObsolete);

            // we cannot migrate resources in 3 cases:
            // 1. Resources does not need migration
            // 2. We are upgrading and already migrated resources and nodes
            // 3. We are downgrading and nodes not yet migrated
            // 4. This is bad idea to upgrade nodes and resources simultaneously
            var cannotMigrateResources = this.currentMigration.Direction == EnMigrationDirection.Stay
                                         || (this.currentMigration.Direction == EnMigrationDirection.Upgrade
                                             && this.resourceState.MigrationState.Position
                                             == EnMigrationActorMigrationPosition.Destination
                                             && this.currentConfiguration.Id == this.currentMigration.ToConfigurationId)
                                         || (this.currentMigration.Direction == EnMigrationDirection.Downgrade
                                             && this.resourceState.MigrationState.Position
                                             == EnMigrationActorMigrationPosition.Source
                                             && this.currentConfiguration.Id == this.currentMigration.FromConfigurationId)
                                         || this.nodeDescriptions.Values.Any(n => n.IsObsolete);

            this.resourceState.CanMigrateResources = !cannotMigrateResources;

            this.resourceState.CanUpdateNodesToDestination =
                this.currentConfiguration.Id == this.currentMigration.FromConfigurationId
                && (this.currentMigration.Direction == EnMigrationDirection.Stay
                    || (this.currentMigration.Direction == EnMigrationDirection.Upgrade
                        && this.resourceState.MigrationState.Position == EnMigrationActorMigrationPosition.Destination)
                    || this.currentMigration.Direction == EnMigrationDirection.Downgrade);

            this.resourceState.CanUpdateNodesToSource = this.currentConfiguration.Id == this.currentMigration.ToConfigurationId
                                                        && (this.currentMigration.Direction == EnMigrationDirection.Stay
                                                            || (this.currentMigration.Direction == EnMigrationDirection
                                                                    .Downgrade && this.resourceState.MigrationState
                                                                    .Position == EnMigrationActorMigrationPosition
                                                                    .Source) || this.currentMigration.Direction
                                                            == EnMigrationDirection.Upgrade);

            this.resourceState.CurrentMigrationStep = this.GetCurrentMigrationStep();
        }

        /// <summary>
        /// Gets the current migration step
        /// </summary>
        /// <returns>The migration step</returns>
        private EnMigrationSteps? GetCurrentMigrationStep()
        {
            if (this.resourceState.CanCancelMigration)
            {
                return EnMigrationSteps.Start;
            }

            if (this.resourceState.CanFinishMigration)
            {
                return EnMigrationSteps.Finish;
            }

            switch (this.currentMigration.Direction)
            {
                case EnMigrationDirection.Upgrade:
                    if (this.resourceState.MigrationState.Position != EnMigrationActorMigrationPosition.Destination)
                    {
                        return EnMigrationSteps.ResourcesUpdating;
                    }
                    else if (this.nodeDescriptions.Values.Any(n => n.IsObsolete))
                    {
                        return EnMigrationSteps.NodesUpdating;
                    }
                    else
                    {
                        return EnMigrationSteps.ResourcesUpdated;
                    }

                case EnMigrationDirection.Downgrade:
                    if (this.nodeDescriptions.Values.Any(n => n.IsObsolete))
                    {
                        return EnMigrationSteps.NodesUpdating;
                    }
                    else if (this.resourceState.MigrationState.Position != EnMigrationActorMigrationPosition.Source)
                    {
                        return EnMigrationSteps.ResourcesUpdating;
                    }
                    else
                    {
                        return EnMigrationSteps.NodesUpdated;
                    }

                case EnMigrationDirection.Stay: return EnMigrationSteps.NodesUpdating;
                default: return EnMigrationSteps.Broken;
            }
        }

        /// <summary>
        /// Gets the list of migration steps
        /// </summary>
        /// <returns>the list of migration steps</returns>
        private IEnumerable<EnMigrationSteps> GetMigrationSteps()
        {
            switch (this.currentMigration.Direction)
            {
                case EnMigrationDirection.Upgrade:
                    yield return EnMigrationSteps.Start;
                    yield return EnMigrationSteps.ResourcesUpdating;
                    yield return EnMigrationSteps.ResourcesUpdated;
                    yield return EnMigrationSteps.NodesUpdating;
                    yield return EnMigrationSteps.Finish;
                    break;
                case EnMigrationDirection.Downgrade:
                    yield return EnMigrationSteps.Start;
                    yield return EnMigrationSteps.NodesUpdating;
                    yield return EnMigrationSteps.NodesUpdated;
                    yield return EnMigrationSteps.ResourcesUpdating;
                    yield return EnMigrationSteps.Finish;
                    break;
                case EnMigrationDirection.Stay:
                    yield return EnMigrationSteps.Start;
                    yield return EnMigrationSteps.NodesUpdating;
                    yield return EnMigrationSteps.Finish;
                    break;
            }
        }

        /// <summary>
        /// Checks current resource state without active migration and/or some resource operation in progress
        /// </summary>
        private void InitResourceReleaseState()
        {
            this.resourceState.MigrationState = null;
            this.resourceState.CurrentMigrationStep = null;
            this.resourceState.MigrationSteps = null;

            this.resourceState.CanUpdateNodesToDestination = false;
            this.resourceState.CanUpdateNodesToSource = false;
            this.resourceState.CanCancelMigration = false;
            this.resourceState.CanFinishMigration = false;

            this.resourceState.CanCreateMigration = true;
            this.resourceState.CanMigrateResources = false;

            if (this.nodeDescriptions.Values.Any(n => n.IsObsolete))
            {
                this.resourceState.CanCreateMigration = false;
            }

            if (this.resourceState.ReleaseState == null)
            {
                this.resourceState.CanCreateMigration = false;
                return;
            }

            // checking if there is any resource that is not of the current release state
            var unmigratedTemplates = this.resourceState.ReleaseState.States
                .Where(t => t.MigratorsStates.Any(m => m.Resources.Any(r => r.CurrentPoint != m.LastDefinedPoint)))
                .ToList();

            if (!unmigratedTemplates.Any())
            {
                return;
            }

            this.resourceState.CanCreateMigration = false;
            this.resourceState.CanMigrateResources = this.resourceState.ReleaseState.States.All(
                t => t.MigratorsStates.All(m => m.Resources.All(r => m.MigrationPoints.Contains(r.CurrentPoint))));
        }

        /// <summary>
        /// Checks current resource state
        /// </summary>
        private void InitResourceState()
        {
            if (this.resourceState.OperationIsInProgress)
            {
                this.resourceState.CanCancelMigration = false;
                this.resourceState.CanCreateMigration = false;
                this.resourceState.CanFinishMigration = false;
                this.resourceState.CanMigrateResources = false;
                this.resourceState.CanUpdateNodesToDestination = false;
                this.resourceState.CanUpdateNodesToSource = false;
                return;
            }

            if (this.currentMigration != null)
            {
                this.InitResourceMigrationState();
            }
            else
            {
                this.InitResourceReleaseState();
            }
        }

        /// <summary>
        /// Receiver actor reviled itself, sending request
        /// </summary>
        /// <param name="actorIdentity">The actor identity</param>
        private void OnActorIdentity(ActorIdentity actorIdentity)
        {
            if ("receiver".Equals(actorIdentity.MessageId as string) && actorIdentity.Subject != null)
            {
                this.Sender.Tell(new NodeDescriptionRequest());
            }
        }

        /// <summary>
        /// Cluster leader has been changed
        /// </summary>
        /// <param name="message">The notification message</param>
        private void OnLeaderChanged(ClusterEvent.LeaderChanged message)
        {
            this.clusterLeader = message.Leader;
            var formerLeader = this.nodeDescriptions.Values.FirstOrDefault(d => d.IsClusterLeader);
            if (formerLeader != null)
            {
                formerLeader.IsClusterLeader = false;
            }

            NodeDescription leader;
            if (message.Leader != null && this.nodeDescriptions.TryGetValue(message.Leader, out leader))
            {
                leader.IsClusterLeader = true;
            }
        }

        /// <summary>
        /// The <see cref="MigrationActor"/> encountered errors on resource check state
        /// </summary>
        /// <param name="state">The initialization error</param>
        private void OnMigrationActorInitializationFailed(MigrationActorInitializationFailed state)
        {
            Context.GetLogger().Error("{Type}: MigrationActor failed to initialize", this.GetType().Name);
            foreach (var error in state.Errors)
            {
                Context.GetLogger().Error(
                    "{Type}: MigrationActor error - {ErrorMessage}\n{ErrorStackTrace}",
                    this.GetType().Name,
                    error.ErrorMessage,
                    error.ErrorStackTrace);
            }

            this.OnMigrationLogRecords(state.Errors.ToList());
            this.resourceState.OperationIsInProgress = false;
            this.resourceState.ReleaseState = null;
            this.resourceState.MigrationState = null;
            this.InitDatabase();
        }

        /// <summary>
        /// The <see cref="MigrationActor"/> updated resource state during active migration
        /// </summary>
        /// <param name="state">The initialization error</param>
        private void OnMigrationActorMigrationState(MigrationActorMigrationState state)
        {
            this.resourceState.OperationIsInProgress = false;
            if (this.currentMigration == null)
            {
                Context.GetLogger().Error(
                    "{Type}: received an MigrationActorMigrationState without active migration",
                    this.GetType().Name);
                return;
            }

            Context.GetLogger().Info("{Type}: received a MigrationActorMigrationState", this.GetType().Name);
            this.resourceState.ReleaseState = null;
            this.resourceState.MigrationState = state;
            this.InitDatabase();
        }

        /// <summary>
        /// The <see cref="MigrationActor"/> updated resource state without active migration
        /// </summary>
        /// <param name="state">The initialization error</param>
        private void OnMigrationActorReleaseState(MigrationActorReleaseState state)
        {
            this.resourceState.OperationIsInProgress = false;
            if (this.currentMigration != null)
            {
                Context.GetLogger().Error(
                    "{Type}: received a MigrationActorReleaseState with active migration in progress",
                    this.GetType().Name);
                return;
            }

            this.resourceState.ReleaseState = state;
            this.resourceState.MigrationState = null;
            Context.GetLogger().Info("{Type}: received a MigrationActorReleaseState", this.GetType().Name);
            this.InitDatabase();
        }

        /// <summary>
        /// Processes the cancel migration request
        /// </summary>
        private void OnMigrationCancel()
        {
            if (this.resourceState.OperationIsInProgress || this.currentMigration == null
                || !this.resourceState.CanCancelMigration)
            {
                Context.GetLogger().Warning(
                    "{Type}: received MigrationCancel request in inappropriate state",
                    this.GetType().Name);
                this.Sender.Tell(false);
                return;
            }

            using (var ds = this.GetContext())
            {
                var migration = ds.Migrations.FirstOrDefault(m => m.Id == this.currentMigration.Id);
                if (migration == null)
                {
                    this.Sender.Tell(false);
                    throw new InvalidOperationException(
                        "while processing MigrationCancel request could not find current migration in database");
                }

                if (!migration.IsActive)
                {
                    this.Sender.Tell(false);
                    throw new InvalidOperationException(
                        "while processing MigrationCancel request current was already not active in database");
                }

                migration.Finished = DateTimeOffset.Now;
                migration.State = EnMigrationState.Failed;
                migration.IsActive = false;
                ds.SaveChanges();
            }

            this.Sender.Tell(true);
            this.currentMigration = null;
            this.resourceMigrator.Tell(new RecheckState(), this.Self);
            this.resourceState.MigrationState = null;
            this.resourceState.ReleaseState = null;
            this.resourceState.OperationIsInProgress = true;
            this.resourceState.CanCreateMigration = false;
            this.resourceState.CanCancelMigration = false;
            this.resourceState.CanFinishMigration = false;
            this.resourceState.CanMigrateResources = false;
            this.resourceState.CanUpdateNodesToDestination = false;
            this.resourceState.CanUpdateNodesToSource = false;
        }

        /// <summary>
        /// Processes the finish migration request
        /// </summary>
        private void OnMigrationFinish()
        {
            if (this.resourceState.OperationIsInProgress || this.currentMigration == null
                || !this.resourceState.CanFinishMigration)
            {
                Context.GetLogger().Warning(
                    "{Type}: received MigrationFinish request in inappropriate state",
                    this.GetType().Name);
                this.Sender.Tell(false);
                return;
            }

            using (var ds = this.GetContext())
            {
                var migration = ds.Migrations.FirstOrDefault(m => m.Id == this.currentMigration.Id);
                if (migration == null)
                {
                    this.Sender.Tell(false);
                    throw new InvalidOperationException(
                        "while processing MigrationFinish request could not find current migration in database");
                }

                if (!migration.IsActive)
                {
                    this.Sender.Tell(false);
                    throw new InvalidOperationException(
                        "while processing MigrationFinish request current was already not active in database");
                }

                migration.Finished = DateTimeOffset.Now;
                migration.State = EnMigrationState.Completed;
                migration.IsActive = false;
                ds.SaveChanges();
            }

            this.Sender.Tell(true);
            this.currentMigration = null;
            this.resourceMigrator.Tell(new RecheckState(), this.Self);
            this.resourceState.MigrationState = null;
            this.resourceState.ReleaseState = null;
            this.resourceState.OperationIsInProgress = true;
            this.resourceState.CanCreateMigration = false;
            this.resourceState.CanCancelMigration = false;
            this.resourceState.CanFinishMigration = false;
            this.resourceState.CanMigrateResources = false;
            this.resourceState.CanUpdateNodesToDestination = false;
            this.resourceState.CanUpdateNodesToSource = false;
        }

        /// <summary>
        /// Received a number of log records to store
        /// </summary>
        /// <param name="records">The records to store</param>
        private void OnMigrationLogRecords(List<MigrationLogRecord> records)
        {
            Context.GetLogger().Info("{Type}: received new MigrationLogRecords", this.GetType().Name);
            using (var ds = this.GetContext())
            {
                foreach (var record in records)
                {
                    ds.MigrationLogs.Add(record);
                    ds.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Processes the request to update cluster nodes during migration process
        /// </summary>
        /// <param name="request">The update request</param>
        private void OnMigrationNodesUpgrade(NodesUpgrade request)
        {
            if (this.resourceState.OperationIsInProgress || this.currentMigration == null
                || (request.Target == EnMigrationSide.Source && !this.resourceState.CanUpdateNodesToSource)
                || (request.Target == EnMigrationSide.Destination && !this.resourceState.CanUpdateNodesToDestination))
            {
                Context.GetLogger().Warning(
                    "{Type}: received NodesUpgrade request in inappropriate state",
                    this.GetType().Name);
                this.Sender.Tell(false);
                return;
            }

            using (var ds = this.GetContext())
            {
                var sourceRelease = ds.Releases.First(r => r.Id == this.currentMigration.FromConfigurationId);
                var destinationRelease = ds.Releases.First(r => r.Id == this.currentMigration.ToConfigurationId);

                if (request.Target == EnMigrationSide.Source)
                {
                    destinationRelease.State = this.currentMigration.Direction == EnMigrationDirection.Downgrade
                                                   ? EnReleaseState.Obsolete
                                                   : EnReleaseState.Faulted;
                    sourceRelease.State = EnReleaseState.Active;
                }
                else
                {
                    destinationRelease.State = EnReleaseState.Active;
                    sourceRelease.State = this.currentMigration.Direction == EnMigrationDirection.Downgrade
                                              ? EnReleaseState.Faulted
                                              : EnReleaseState.Obsolete;
                }

                ds.SaveChanges();
                this.GetCurrentRelease(ds);
            }

            this.Sender.Tell(true);
            foreach (var nodeDescription in this.nodeDescriptions.Values)
            {
                this.CheckNodeIsObsolete(nodeDescription);
            }

            this.OnNodeUpgrade();
            this.InitResourceState();
        }

        /// <summary>
        /// Processes the request to upgrade resources during migration
        /// </summary>
        /// <param name="request">The request</param>
        private void OnMigrationResourceUpgrade(List<ResourceUpgrade> request)
        {
            if (this.resourceState.OperationIsInProgress || this.currentMigration == null
                || !this.resourceState.CanMigrateResources || this.resourceState.MigrationState == null)
            {
                Context.GetLogger().Warning(
                    "{Type}: received ResourceUpgrade request in inappropriate state",
                    this.GetType().Name);
                this.Sender.Tell(false);
                return;
            }

            foreach (var upgrade in request)
            {
                var template =
                    this.resourceState.MigrationState.TemplateStates
                        .FirstOrDefault(t => t.Code == upgrade.TemplateCode);
                var migrator = template?.Migrators.FirstOrDefault(m => m.TypeName == upgrade.MigratorTypeName);
                var resource = migrator?.Resources.FirstOrDefault(r => r.Code == upgrade.ResourceCode);
                if (resource == null)
                {
                    Context.GetLogger().Warning(
                        "{Type}: received ResourceUpgrade request with unknown resource {TemplateCode} {MigratorTypeName} {ResourceCode}",
                        this.GetType().Name,
                        upgrade.TemplateCode,
                        upgrade.MigratorTypeName,
                        upgrade.ResourceCode);
                    this.Sender.Tell(false);
                    return;
                }
            }

            this.Sender.Tell(true);
            this.resourceMigrator.Tell(request, this.Self);
            this.resourceState.OperationIsInProgress = true;
            this.resourceState.CanCreateMigration = false;
            this.resourceState.CanCancelMigration = false;
            this.resourceState.CanFinishMigration = false;
            this.resourceState.CanMigrateResources = false;
            this.resourceState.CanUpdateNodesToDestination = false;
            this.resourceState.CanUpdateNodesToSource = false;
            this.resourceState.CurrentMigrationStep = EnMigrationSteps.ResourcesUpdating;
        }

        /// <summary>
        /// There is new node going to be up and we need to decide what template should be applied
        /// </summary>
        /// <param name="request">The node template request</param>
        private void OnNewNodeTemplateRequest(NewNodeTemplateRequest request)
        {
            var templates = this.GetPossibleTemplatesForContainer(request.ContainerType, request.FrameworkRuntimeType);

            if (templates.Count == 0)
            {
                // Cluster is full, we don't need any new nodes
                this.Sender.Tell(new NodeStartupWaitMessage { WaitTime = this.fullClusterWaitTimeout });
                return;
            }

            var dice = this.random.NextDouble();
            var sumWeight = templates.Sum(t => t.Priority);

            var check = 0.0;
            NodeTemplate selectedNodeTemplate = null;
            foreach (var template in templates)
            {
                check += template.Priority / sumWeight;
                if (dice <= check)
                {
                    selectedNodeTemplate = template;
                    break;
                }
            }

            // this could never happen, but code analyzers can't understand it
            if (selectedNodeTemplate == null)
            {
                Context.GetLogger().Warning("{Type}: Failed to select nodeTemplate with dice", this.GetType().Name);
                selectedNodeTemplate = templates.Last();
            }

            this.Sender.Tell(
                new NodeStartUpConfiguration
                    {
                        NodeTemplate = selectedNodeTemplate.Code,
                        ReleaseId = this.currentConfiguration.Id,
                        Configuration = selectedNodeTemplate.Configuration,
                        Seeds = this.currentConfiguration.Settings.SeedAddresses
                            .OrderBy(s => this.random.NextDouble()).ToList(),
                        Packages =
                            selectedNodeTemplate.PackagesToInstall[request.FrameworkRuntimeType],
                        PackageSource = this.currentConfiguration.Settings.NugetFeed
                    });

            List<Guid> requests;
            if (!this.awaitingRequestsByTemplate.TryGetValue(selectedNodeTemplate.Code, out requests))
            {
                requests = new List<Guid>();
                this.awaitingRequestsByTemplate[selectedNodeTemplate.Code] = requests;
            }

            requests.Add(request.NodeUid);

            Context.System.Scheduler.ScheduleTellOnce(
                this.newNodeJoinTimeout,
                this.Self,
                new RequestTimeOut { NodeId = request.NodeUid, TemplateCode = selectedNodeTemplate.Code },
                this.Self);
        }

        /// <summary>
        /// On new node description
        /// </summary>
        /// <param name="nodeDescription">Node description</param>
        private void OnNodeDescription(NodeDescription nodeDescription)
        {
            Cancelable cancelable;
            var address = nodeDescription.NodeAddress;
            if (nodeDescription.NodeAddress == null)
            {
                Context.GetLogger().Warning(
                    "{Type}: received nodeDescription with null address from {NodeAddress}",
                    this.GetType().Name,
                    this.Sender.Path.Address.ToString());
                return;
            }

            if (!this.requestDescriptionNotifications.TryGetValue(address, out cancelable))
            {
                Context.GetLogger().Warning(
                    "{Type}: received nodeDescription from unknown node with address {NodeAddress}",
                    this.GetType().Name,
                    address.ToString());
                return;
            }

            cancelable.Cancel();

            this.upgradingNodes.Remove(nodeDescription.NodeId);
            this.CheckNodeIsObsolete(nodeDescription);

            this.requestDescriptionNotifications.Remove(address);
            this.nodeDescriptions[address] = nodeDescription;

            nodeDescription.IsClusterLeader = this.clusterLeader == address;
            nodeDescription.LeaderInRoles = this.roleLeaders.Where(p => p.Value == address).Select(p => p.Key).ToList();

            if (!string.IsNullOrEmpty(nodeDescription.NodeTemplate))
            {
                List<Address> nodeList;
                if (!this.activeNodesByTemplate.TryGetValue(nodeDescription.NodeTemplate, out nodeList))
                {
                    nodeList = new List<Address>();
                    this.activeNodesByTemplate[nodeDescription.NodeTemplate] = nodeList;
                }

                nodeList.Add(nodeDescription.NodeAddress);
            }

            if (!string.IsNullOrWhiteSpace(nodeDescription.NodeTemplate))
            {
                List<Guid> awaitingRequests;
                if (this.awaitingRequestsByTemplate.TryGetValue(nodeDescription.NodeTemplate, out awaitingRequests))
                {
                    awaitingRequests.Remove(nodeDescription.NodeId);
                }

                Context.GetLogger().Info(
                    "{Type}: New node {NodeTemplateName} on address {NodeAddress}",
                    this.GetType().Name,
                    nodeDescription.NodeTemplate,
                    address.ToString());
            }
            else
            {
                Context.GetLogger().Info(
                    "{Type}: New node without node template on address {NodeAddress}",
                    this.GetType().Name,
                    address.ToString());
            }

            this.Self.Tell(new UpgradeMessage());
            this.InitResourceState();
        }

        /// <summary>
        /// Processes node removed cluster event
        /// </summary>
        /// <param name="member">Obsolete node</param>
        private void OnNodeDown(Member member)
        {
            var address = member.Address;
            Cancelable cancelable;
            if (this.requestDescriptionNotifications.TryGetValue(address, out cancelable))
            {
                cancelable.Cancel();
                this.requestDescriptionNotifications.Remove(address);
            }

            NodeDescription nodeDescription;

            if (this.nodeDescriptions.TryGetValue(address, out nodeDescription))
            {
                nodeDescription = this.nodeDescriptions[address];
                this.nodeDescriptions.Remove(address);

                if (!string.IsNullOrWhiteSpace(nodeDescription?.NodeTemplate))
                {
                    this.activeNodesByTemplate[nodeDescription.NodeTemplate].Remove(address);
                }
            }
            else
            {
                Context.GetLogger().Warning(
                    "{Type}: received node down for unknown node with address {NodeAddress}",
                    this.GetType().ToString(),
                    address.ToString());
            }
        }

        /// <summary>
        /// Processes new node cluster event
        /// </summary>
        /// <param name="member">New node</param>
        private void OnNodeUp(Member member)
        {
            var address = member.Address;
            var cancelable = new Cancelable(Context.System.Scheduler);
            this.requestDescriptionNotifications[address] = cancelable;

            if (!this.nodeDescriptions.ContainsKey(address))
            {
                var nodeDescription = new NodeDescription
                                          {
                                              NodeId = Guid.NewGuid(),
                                              IsInitialized = false,
                                              NodeAddress = address,
                                              Modules = new List<PackageDescription>(),
                                              Roles = new List<string>(member.Roles),
                                              LeaderInRoles =
                                                  this.roleLeaders.Where(p => p.Value == address)
                                                      .Select(p => p.Key).ToList(),
                                              IsClusterLeader = address == this.clusterLeader
                                          };

                this.nodeDescriptions.Add(address, nodeDescription);
            }

            this.OnRequestDescriptionNotification(new RequestDescriptionNotification { Address = address });
        }

        /// <summary>
        /// Process of manual node upgrade request
        /// </summary>
        /// <param name="address">Address of the node to upgrade</param>
        /// <param name="sendResult">A value indicating whether to send result of operation to the sender</param>
        private void OnNodeUpdateRequest(Address address, bool sendResult)
        {
            if (!this.nodeDescriptions.ContainsKey(address))
            {
                if (sendResult)
                {
                    this.Sender.Tell(false);
                }

                return;
            }

            this.router.Tell(address, "/user/NodeManager/Receiver", new ShutdownMessage(), this.Self);

            if (sendResult)
            {
                this.Sender.Tell(true);
            }
        }

        /// <summary>
        /// Checks current nodes for possible upgrade need and performs an upgrade
        /// </summary>
        private void OnNodeUpgrade()
        {
            this.upgradeMessageSchedule?.Cancel();

            var upgradeTimeOut = this.newNodeJoinTimeout + this.newNodeRequestDescriptionNotificationTimeout;

            // removing lost nodes
            var obsoleteUpgrades = this.upgradingNodes.Values
                .Where(u => DateTimeOffset.Now - u.UpgradeStartTime >= upgradeTimeOut).ToList();

            obsoleteUpgrades.ForEach(u => this.upgradingNodes.Remove(u.NodeId));
            var isUpgrading = false;

            // searching for nodes to upgrade
            var groupedNodes = this.nodeDescriptions.Values.GroupBy(n => n.NodeTemplate);
            foreach (var nodeGroup in groupedNodes)
            {
                if (!nodeGroup.Any(n => n.IsObsolete))
                {
                    continue;
                }

                var nodeTemplate =
                    this.currentConfiguration.Settings.NodeTemplates.FirstOrDefault(t => t.Code == nodeGroup.Key);

                int nodesToUpgradeCount;
                if (nodeTemplate == null)
                {
                    nodeGroup.ForEach(n => n.IsObsolete = true);
                    nodesToUpgradeCount = nodeGroup.Count();
                }
                else
                {
                    if (nodeGroup.Count() <= nodeTemplate.MinimumRequiredInstances)
                    {
                        // node upgrade is blocked if it can cause cluster malfunction
                        continue;
                    }

                    var nodesInUpgrade = this.upgradingNodes.Values.Count(u => u.NodeTemplate == nodeGroup.Key);

                    nodesToUpgradeCount = (int)Math.Ceiling(nodeGroup.Count() * this.upgradablePart / 100.0M)
                                          - nodesInUpgrade;

                    if (nodesToUpgradeCount <= 0)
                    {
                        continue;
                    }
                }

                var nodes = nodeGroup.Where(n => n.IsObsolete).OrderBy(n => n.StartTimeStamp).Take(nodesToUpgradeCount);
                isUpgrading = true;
                foreach (var node in nodes)
                {
                    this.upgradingNodes[node.NodeId] =
                        new UpgradeData
                            {
                                NodeId = node.NodeId,
                                NodeTemplate = node.NodeTemplate,
                                UpgradeStartTime = DateTimeOffset.Now
                            };
                    this.OnNodeUpdateRequest(node.NodeAddress, false);
                }
            }

            if (isUpgrading)
            {
                this.upgradeMessageSchedule = new Cancelable(Context.System.Scheduler);
                Context.System.Scheduler.ScheduleTellOnce(
                    upgradeTimeOut.Add(TimeSpan.FromSeconds(1)),
                    this.Self,
                    new UpgradeMessage(),
                    this.Self,
                    this.upgradeMessageSchedule);
            }
        }

        /// <summary>
        /// Sends new description request to foreign node
        /// </summary>
        /// <param name="message">Notification message</param>
        private void OnRequestDescriptionNotification(RequestDescriptionNotification message)
        {
            Cancelable cancelable;
            if (!this.requestDescriptionNotifications.TryGetValue(message.Address, out cancelable))
            {
                return;
            }

            this.router.Tell(message.Address, "/user/NodeManager/Receiver", new Identify("receiver"), this.Self);

            if (message.Attempt >= this.newNodeRequestDescriptionNotificationMaxRequests)
            {
                return;
            }

            var notification =
                new RequestDescriptionNotification { Address = message.Address, Attempt = message.Attempt + 1 };

            Context.System.Scheduler.ScheduleTellOnce(
                this.newNodeRequestDescriptionNotificationTimeout,
                this.Self,
                notification,
                this.Self,
                cancelable);
        }

        /// <summary>
        /// Process the event of <seealso cref="NodeStartUpConfiguration"/> time out
        /// </summary>
        /// <param name="requestTimeOut">The timeout notification</param>
        private void OnRequestTimeOut(RequestTimeOut requestTimeOut)
        {
            List<Guid> requests;
            if (this.awaitingRequestsByTemplate.TryGetValue(requestTimeOut.TemplateCode, out requests))
            {
                requests.Remove(requestTimeOut.NodeId);
            }
        }

        /// <summary>
        /// The role leader has been changed
        /// </summary>
        /// <param name="message">The notification message</param>
        private void OnRoleLeaderChanged(ClusterEvent.RoleLeaderChanged message)
        {
            if (string.IsNullOrWhiteSpace(message.Role))
            {
                Context.GetLogger().Warning(
                    "{Type}: received RoleLeaderChanged message with empty role",
                    this.GetType().ToString());
                return;
            }

            var formerLeader = this.nodeDescriptions.Values.FirstOrDefault(d => d.LeaderInRoles.Contains(message.Role));
            formerLeader?.LeaderInRoles.Remove(message.Role);

            if (message.Leader == null)
            {
                this.roleLeaders.Remove(message.Role);
            }
            else
            {
                this.roleLeaders[message.Role] = message.Leader;
                NodeDescription leader;
                if (this.nodeDescriptions.TryGetValue(message.Leader, out leader))
                {
                    leader.LeaderInRoles.Add(message.Role);
                }
            }
        }

        /// <summary>
        /// Process the <seealso cref="TemplatesStatisticsRequest"/> request
        /// </summary>
        private void OnTemplatesStatisticsRequest()
        {
            Func<NodeTemplate, TemplatesUsageStatistics.Data> selector =
                t => new TemplatesUsageStatistics.Data
                         {
                             MaximumRequiredNodes = t.MaximumNeededInstances,
                             MinimumRequiredNodes = t.MinimumRequiredInstances,
                             Name = t.Code,
                             ActiveNodes =
                                 this.activeNodesByTemplate.ContainsKey(t.Code)
                                     ? this.activeNodesByTemplate[t.Code].Count
                                     : 0,
                             ObsoleteNodes =
                                 this.activeNodesByTemplate.ContainsKey(t.Code)
                                     ? this.activeNodesByTemplate[t.Code].Count(
                                         a => this.nodeDescriptions[a].IsObsolete)
                                     : 0,
                             UpgradingNodes =
                                 this.upgradingNodes.Values.Count(
                                     d => d.NodeTemplate == t.Code),
                             StartingNodes =
                                 this.awaitingRequestsByTemplate.ContainsKey(t.Code)
                                     ? this.awaitingRequestsByTemplate[t.Code].Count
                                     : 0,
                         };
            var stats = new TemplatesUsageStatistics
                            {
                                Templates = this.currentConfiguration.Settings.NodeTemplates
                                    .Select(selector).ToList()
                            };

            this.Sender.Tell(stats, this.Self);
        }

        /// <summary>
        /// Initiates the cluster migration process to the new release
        /// </summary>
        /// <param name="request">The request</param>
        private void OnUpdateCluster(UpdateClusterRequest request)
        {
            if (this.resourceState.OperationIsInProgress)
            {
                this.Sender.Tell(
                    CrudActionResponse<Migration>.Error(new Exception("Resources are still checking"), null));
                return;
            }

            if (this.resourceState.ReleaseState == null)
            {
                this.Sender.Tell(
                    CrudActionResponse<Migration>.Error(new Exception("Resources state is unknown"), null));
                return;
            }

            if (!this.resourceState.CanCreateMigration)
            {
                this.Sender.Tell(
                    CrudActionResponse<Migration>.Error(
                        new Exception("The migration cannot be created at this time"),
                        null));
                return;
            }

            try
            {
                using (var ds = this.GetContext())
                {
                    var activeMigration = ds.Migrations.FirstOrDefault(m => m.IsActive);
                    if (activeMigration != null)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Migration>.Error(
                                new Exception("There is already a pending migration"),
                                null));
                        return;
                    }

                    if (request.Id == this.currentConfiguration.Id)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Migration>.Error(new Exception("This release is already set"), null));
                        return;
                    }

                    var newRelease = ds.Releases.FirstOrDefault(r => r.Id == request.Id);
                    if (newRelease == null)
                    {
                        this.Sender.Tell(CrudActionResponse<Migration>.Error(new EntityNotFoundException(), null));
                        return;
                    }

                    if (newRelease.State == EnReleaseState.Draft)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<Migration>.Error(
                                new Exception("cluster cannot be migrated to draft release"),
                                null));
                        return;
                    }

                    var migration = new Migration
                                        {
                                            State = EnMigrationState.Preparing,
                                            Started = DateTimeOffset.Now,
                                            IsActive = true,
                                            FromConfigurationId = this.currentConfiguration.Id,
                                            ToConfigurationId = newRelease.Id
                                        };
                    ds.Migrations.Add(migration);
                    ds.SaveChanges();
                    this.currentMigration = migration;
                    this.resourceState.OperationIsInProgress = true;
                    this.resourceState.CanCancelMigration = false;
                    this.resourceState.CanCreateMigration = false;
                    this.resourceState.CanFinishMigration = false;
                    this.resourceState.CanMigrateResources = false;
                    this.resourceState.CanUpdateNodesToDestination = false;
                    this.resourceState.CanUpdateNodesToSource = false;
                    this.resourceState.ReleaseState = null;
                    this.resourceState.MigrationState = null;
                    this.resourceMigrator.Tell(new RecheckState(), this.Self);
                    this.Sender.Tell(CrudActionResponse<Migration>.Success(migration, null));
                }
            }
            catch (Exception exception)
            {
                this.Sender.Tell(CrudActionResponse<Migration>.Error(exception, null));
            }
        }

        /// <summary>
        /// Initializes normal actor work
        /// </summary>
        private void Start()
        {
            Cluster.Get(Context.System).Subscribe(
                this.Self,
                ClusterEvent.InitialStateAsEvents,
                typeof(ClusterEvent.MemberRemoved),
                typeof(ClusterEvent.MemberUp),
                typeof(ClusterEvent.LeaderChanged),
                typeof(ClusterEvent.RoleLeaderChanged));

            // ping message will indicate that actor started and ready to work
            this.Receive<PingMessage>(m => this.Sender.Tell(new PongMessage()));

            this.Receive<ClusterEvent.MemberUp>(m => this.OnNodeUp(m.Member));
            this.Receive<ClusterEvent.MemberRemoved>(m => this.OnNodeDown(m.Member));

            this.Receive<ClusterEvent.LeaderChanged>(m => this.OnLeaderChanged(m));
            this.Receive<ClusterEvent.RoleLeaderChanged>(m => this.OnRoleLeaderChanged(m));

            this.Receive<RequestDescriptionNotification>(m => this.OnRequestDescriptionNotification(m));
            this.Receive<NodeDescription>(m => this.OnNodeDescription(m));
            this.Receive<ActiveNodeDescriptionsRequest>(
                m => { this.Sender.Tell(this.nodeDescriptions.Values.ToList()); });
            this.Receive<ActorIdentity>(m => this.OnActorIdentity(m));
            this.Receive<NodeUpgradeRequest>(m => this.OnNodeUpdateRequest(m.Address, true));

            this.Receive<NewNodeTemplateRequest>(m => this.OnNewNodeTemplateRequest(m));
            this.Receive<RequestTimeOut>(m => this.OnRequestTimeOut(m));

            this.Receive<UpgradeMessage>(m => this.OnNodeUpgrade());
            this.Receive<AvailableTemplatesRequest>(
                m => this.Sender.Tell(this.GetPossibleTemplatesForContainer(m.ContainerType, m.FrameworkRuntimeType)));
            this.Receive<TemplatesStatisticsRequest>(m => this.OnTemplatesStatisticsRequest());

            this.Receive<AuthenticateUserWithCredentials>(m => this.workers.Forward(m));
            this.Receive<AuthenticateUserWithUid>(m => this.workers.Forward(m));
            this.Receive<CollectionRequest<User>>(m => this.workers.Forward(m));
            this.Receive<CrudActionMessage<User, Guid>>(m => this.workers.Forward(m));
            this.Receive<CollectionRequest<Role>>(m => this.workers.Forward(m));
            this.Receive<CrudActionMessage<Role, Guid>>(m => this.workers.Forward(m));
            this.Receive<UserChangePasswordRequest>(m => this.workers.Forward(m));
            this.Receive<UserResetPasswordRequest>(m => this.workers.Forward(m));
            this.Receive<UserRoleAddRequest>(m => this.workers.Forward(m));
            this.Receive<UserRoleRemoveRequest>(m => this.workers.Forward(m));

            this.Receive<CollectionRequest<Configuration>>(m => this.workers.Forward(m));

            this.Receive<CrudActionMessage<Configuration, int>>(m => this.releaseManager.Forward(m));
            this.Receive<ReleaseCheckRequest>(m => this.releaseManager.Forward(m));
            this.Receive<ReleaseSetReadyRequest>(m => this.releaseManager.Forward(m));
            this.Receive<ReleaseSetObsoleteRequest>(m => this.releaseManager.Forward(m));
            this.Receive<ReleaseSetStableRequest>(m => this.releaseManager.Forward(m));

            this.Receive<UpdateClusterRequest>(r => this.OnUpdateCluster(r));
            this.Receive<CollectionRequest<Migration>>(m => this.workers.Forward(m));

            this.Receive<ProcessingTheRequest>(m => this.resourceState.OperationIsInProgress = true);
            this.Receive<MigrationActorMigrationState>(s => this.OnMigrationActorMigrationState(s));
            this.Receive<MigrationActorReleaseState>(s => this.OnMigrationActorReleaseState(s));
            this.Receive<MigrationActorInitializationFailed>(r => this.OnMigrationActorInitializationFailed(r));
            this.Receive<List<MigrationLogRecord>>(r => this.OnMigrationLogRecords(r));
            this.Receive<MigrationCancel>(r => this.OnMigrationCancel());
            this.Receive<MigrationFinish>(r => this.OnMigrationFinish());
            this.Receive<List<ResourceUpgrade>>(r => this.OnMigrationResourceUpgrade(r));
            this.Receive<NodesUpgrade>(r => this.OnMigrationNodesUpgrade(r));

            this.Receive<ResourceStateRequest>(r => this.Sender.Tell(this.resourceState));
            this.Receive<CurrentReleaseRequest>(r => this.Sender.Tell(this.currentConfiguration));
            this.Receive<CurrentMigrationRequest>(r => this.Sender.Tell(new Maybe<Migration>(this.currentMigration)));
        }

        /// <summary>
        /// Message used for self initialization
        /// </summary>
        private class InitializationMessage
        {
        }

        /// <summary>
        /// Self notification to make additional request to get node description
        /// </summary>
        private class RequestDescriptionNotification
        {
            /// <summary>
            /// Gets or sets address of the node
            /// </summary>
            public Address Address { get; set; }

            /// <summary>
            /// Gets or sets request attempt number
            /// </summary>
            public int Attempt { get; set; }
        }

        /// <summary>
        /// Notification of <seealso cref="NodeStartUpConfiguration"/> request timeout.
        /// </summary>
        private class RequestTimeOut
        {
            /// <summary>
            /// Gets or sets nodes unique id
            /// </summary>
            public Guid NodeId { get; set; }

            /// <summary>
            /// Gets or sets code of <seealso cref="NodeTemplate"/> assigned to request
            /// </summary>
            public string TemplateCode { get; set; }
        }

        /// <summary>
        /// Information about node, that was sent to upgrade
        /// </summary>
        private class UpgradeData
        {
            /// <summary>
            /// Gets or sets node unique identification number
            /// </summary>
            public Guid NodeId { get; set; }

            /// <summary>
            /// Gets or sets node template code before upgrade
            /// </summary>
            public string NodeTemplate { get; set; }

            /// <summary>
            /// Gets or sets time of upgrade initialization process
            /// </summary>
            public DateTimeOffset UpgradeStartTime { get; set; }
        }

        /// <summary>
        /// Message used for self check and initiate node upgrade
        /// </summary>
        private class UpgradeMessage
        {
        }

        /// <summary>
        /// Child actor intended to process database requests related to <seealso cref="NodeTemplate"/>
        /// </summary>
        private class Worker : BaseCrudActorWithNotifications<ConfigurationContext>
        {
            /// <summary>
            /// The database provider name
            /// </summary>
            private readonly string databaseProviderName;

            /// <summary>
            /// The database connection string
            /// </summary>
            private readonly string connectionString;

            /// <summary>
            /// Configuration context factory
            /// </summary>
            private readonly UniversalContextFactory contextFactory;

            /// <summary>
            /// The database name
            /// </summary>
            private readonly string databaseName;

            /// <summary>
            /// Initializes a new instance of the <see cref="Worker"/> class.
            /// </summary>
            /// <param name="databaseProviderName">
            /// The database provider name
            /// </param>
            /// <param name="connectionString">
            /// The database connection string
            /// </param>
            /// <param name="databaseName">
            /// The database name
            /// </param>
            /// <param name="componentContext">
            /// The DI context
            /// </param>
            /// <param name="contextFactory">
            /// Configuration context factory
            /// </param>
            /// <param name="parent">
            /// Reference to the <seealso cref="NodeManagerActor"/>
            /// </param>
            public Worker(
                string databaseProviderName,
                string connectionString,
                string databaseName,
                IComponentContext componentContext,
                UniversalContextFactory contextFactory,
                IActorRef parent)
                : base(componentContext, parent)
            {
                // ReSharper disable FormatStringProblem
                Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());

                // ReSharper restore FormatStringProblem
                this.databaseProviderName = databaseProviderName;
                this.connectionString = connectionString;
                this.databaseName = databaseName;
                this.contextFactory = contextFactory;

                this.ReceiveAsync<CrudActionMessage<User, Guid>>(this.OnRequest);
                this.ReceiveAsync<CrudActionMessage<Role, Guid>>(this.OnRequest);
                this.ReceiveAsync<CrudActionMessage<Configuration, int>>(this.OnRequest);

                this.ReceiveAsync<CollectionRequest<User>>(this.OnCollectionRequest<User, Guid>);
                this.ReceiveAsync<CollectionRequest<Role>>(this.OnCollectionRequest<Role, Guid>);
                this.ReceiveAsync<CollectionRequest<Configuration>>(this.OnCollectionRequest<Configuration, int>);
                this.ReceiveAsync<CollectionRequest<Migration>>(this.OnCollectionRequest<Migration, int>);

                this.Receive<UserChangePasswordRequest>(r => this.OnUserChangePassword(r));
                this.Receive<UserResetPasswordRequest>(r => this.OnUserResetPassword(r));
                this.Receive<UserRoleAddRequest>(r => this.OnUserRoleAdd(r));
                this.Receive<UserRoleRemoveRequest>(r => this.UserRoleRemove(r));

                this.ReceiveAsync<AuthenticateUserWithCredentials>(this.AuthenticateUser);
                this.ReceiveAsync<AuthenticateUserWithUid>(this.AuthenticateUser);
            }

            /// <summary>
            /// Opens new database connection and generates execution context
            /// </summary>
            /// <returns>New working context</returns>
            protected override ConfigurationContext GetContext()
            {
                return this.contextFactory.CreateContext<ConfigurationContext>(
                    this.databaseProviderName,
                    this.connectionString,
                    this.databaseName);
            }

            /// <summary>
            /// Authenticate user by it's credentials
            /// </summary>
            /// <param name="request">The request</param>
            /// <returns>The async task</returns>
            private async Task AuthenticateUser(AuthenticateUserWithCredentials request)
            {
                using (var ds = this.GetContext())
                {
                    var factory = DataFactory<ConfigurationContext, User, string>.CreateFactory(this.ComponentContext, ds);
                    try
                    {
                        var user = await factory.Get(request.Login);
                        if (!user.HasValue || !user.Value.CheckPassword(request.Password))
                        {
                            this.Sender.Tell(CrudActionResponse<User>.Error(new EntityNotFoundException(), null));
                        }

                        this.Sender.Tell(CrudActionResponse<User>.Success(user, null));
                    }
                    catch (Exception exception)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<User>.Error(
                                new DatasourceInnerException(
                                    "Exception on AuthenticateUserWithCredentials operation",
                                    exception),
                                null));
                    }
                }
            }

            /// <summary>
            /// Authenticate user by it's uid
            /// </summary>
            /// <param name="request">The request</param>
            /// <returns>The async task</returns>
            private async Task AuthenticateUser(AuthenticateUserWithUid request)
            {
                using (var ds = this.GetContext())
                {
                    var factory = DataFactory<ConfigurationContext, User, Guid>.CreateFactory(this.ComponentContext, ds);
                    try
                    {
                        var user = await factory.Get(request.Uid);
                        if (!user.HasValue)
                        {
                            this.Sender.Tell(CrudActionResponse<User>.Error(new EntityNotFoundException(), null));
                        }

                        this.Sender.Tell(CrudActionResponse<User>.Success(user, null));
                    }
                    catch (Exception exception)
                    {
                        this.Sender.Tell(
                            CrudActionResponse<User>.Error(
                                new DatasourceInnerException(
                                    "Exception on AuthenticateUserWithCredentials operation",
                                    exception),
                                null));
                    }
                }
            }

            /// <summary>
            /// Process the <see cref="UserChangePasswordRequest"/>
            /// </summary>
            /// <param name="request">The request</param>
            private void OnUserChangePassword(UserChangePasswordRequest request)
            {
                try
                {
                    using (var ds = this.GetContext())
                    {
                        var user = ds.Users.FirstOrDefault(u => u.Login == request.Login);
                        if (user == null)
                        {
                            var errors = new List<ErrorDescription> { new ErrorDescription("login", "not found") };

                            this.Sender.Tell(new MutationResult<bool> { Errors = errors });
                            return;
                        }

                        if (!user.CheckPassword(request.OldPassword))
                        {
                            var errors = new List<ErrorDescription> { new ErrorDescription("login", "not found") };

                            this.Sender.Tell(new MutationResult<bool> { Errors = errors });
                            return;
                        }

                        user.SetPassword(request.NewPassword);
                        ds.SaveChanges();

                        SecurityLog.CreateRecord(
                            EnSecurityLogType.DataUpdateGranted,
                            EnSeverity.Trivial,
                            request.Request,
                            "User {Login} ({Uid}) have changed his password",
                            user.Login,
                            user.Uid);
                        this.Sender.Tell(new MutationResult<bool> { Result = true });
                    }
                }
                catch (Exception exception)
                {
                    var errors = new List<ErrorDescription> { new ErrorDescription(null, exception.Message) };
                    this.Sender.Tell(new MutationResult<bool> { Errors = errors });
                }
            }

            /// <summary>
            /// Process the <see cref="UserResetPasswordRequest"/>
            /// </summary>
            /// <param name="request">The request</param>
            private void OnUserResetPassword(UserResetPasswordRequest request)
            {
                try
                {
                    using (var ds = this.GetContext())
                    {
                        var user = ds.Users.FirstOrDefault(u => u.Uid == request.UserUid);
                        if (user == null)
                        {
                            this.Sender.Tell(CrudActionResponse<User>.Error(new EntityNotFoundException(), null));
                            return;
                        }

                        user.SetPassword(request.NewPassword);
                        ds.SaveChanges();

                        SecurityLog.CreateRecord(
                            EnSecurityLogType.DataUpdateGranted,
                            EnSeverity.Crucial,
                            request.Request,
                            "The password for user {Login} ({Uid}) was reset",
                            user.Login,
                            user.Uid);

                        this.Sender.Tell(CrudActionResponse<User>.Success(user, request.ExtraData));
                    }
                }
                catch (Exception exception)
                {
                    this.Sender.Tell(CrudActionResponse<User>.Error(exception, null));
                }
            }

            /// <summary>
            /// Process the <see cref="UserRoleAddRequest"/>
            /// </summary>
            /// <param name="request">The request</param>
            private void OnUserRoleAdd(UserRoleAddRequest request)
            {
                try
                {
                    using (var ds = this.GetContext())
                    {
                        var user = ds.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Uid == request.UserUid);
                        var role = ds.Roles.Include(nameof(Role.Users)).FirstOrDefault(r => r.Uid == request.RoleUid);

                        if (user == null || role == null)
                        {
                            this.Sender.Tell(
                                request.ReturnUser
                                    ? (object)CrudActionResponse<User>.Error(
                                        new EntityNotFoundException(),
                                        request.ExtraData)
                                    : CrudActionResponse<Role>.Error(new EntityNotFoundException(), request.ExtraData));
                            return;
                        }

                        if (user.Roles.Any(r => r.RoleUid == role.Uid))
                        {
                            var exception =
                                new MutationException(new ErrorDescription(null, "The role is already granted"));
                            this.Sender.Tell(
                                request.ReturnUser
                                    ? (object)CrudActionResponse<User>.Error(exception, request.ExtraData)
                                    : CrudActionResponse<Role>.Error(exception, request.ExtraData));
                            return;
                        }

                        user.Roles.Add(new RoleUser { RoleUid = role.Uid });
                        ds.SaveChanges();
                        SecurityLog.CreateRecord(
                            EnSecurityLogType.DataUpdateGranted,
                            EnSeverity.Crucial,
                            request.Request,
                            "The user {Login} ({UserUid}) was granted with role {RoleName} ({RoleUid})",
                            user.Login,
                            user.Uid,
                            role.Name,
                            role.Uid);
                    }

                    using (var ds = this.GetContext())
                    {
                        this.Sender.Tell(
                            request.ReturnUser
                                ? (object)CrudActionResponse<User>.Success(
                                    ds.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Uid == request.UserUid),
                                    request.ExtraData)
                                : CrudActionResponse<Role>.Success(
                                    ds.Roles.Include(nameof(Role.Users)).FirstOrDefault(r => r.Uid == request.RoleUid),
                                    request.ExtraData));
                    }
                }
                catch (Exception exception)
                {
                    this.Sender.Tell(
                        request.ReturnUser
                            ? (object)CrudActionResponse<User>.Error(exception, request.ExtraData)
                            : CrudActionResponse<Role>.Error(exception, request.ExtraData));
                }
            }

            /// <summary>
            /// Process the <see cref="UserRoleRemoveRequest"/>
            /// </summary>
            /// <param name="request">The request</param>
            private void UserRoleRemove(UserRoleRemoveRequest request)
            {
                try
                {
                    using (var ds = this.GetContext())
                    {
                        var user = ds.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Uid == request.UserUid);
                        var role = ds.Roles.Include(nameof(Role.Users)).FirstOrDefault(r => r.Uid == request.RoleUid);

                        if (user == null || role == null)
                        {
                            this.Sender.Tell(
                                request.ReturnUser
                                    ? (object)CrudActionResponse<User>.Error(
                                        new EntityNotFoundException(),
                                        request.ExtraData)
                                    : CrudActionResponse<Role>.Error(new EntityNotFoundException(), request.ExtraData));
                            return;
                        }

                        if (user.Roles.All(r => r.RoleUid != role.Uid))
                        {
                            var exception =
                                new MutationException(new ErrorDescription(null, "The role is not granted"));
                            this.Sender.Tell(
                                request.ReturnUser
                                    ? (object)CrudActionResponse<User>.Error(exception, request.ExtraData)
                                    : CrudActionResponse<Role>.Error(exception, request.ExtraData));
                            return;
                        }

                        ds.RoleUsers.Remove(ds.RoleUsers.First(ru => ru.RoleUid == role.Uid && ru.UserUid == user.Uid));

                        ds.SaveChanges();
                        SecurityLog.CreateRecord(
                            EnSecurityLogType.DataUpdateGranted,
                            EnSeverity.Crucial,
                            request.Request,
                            "The role {RoleName} ({RoleUid}) was withdrawn from user {Login} ({UserUid})",
                            role.Name,
                            role.Uid,
                            user.Login,
                            user.Uid);
                    }

                    using (var ds = this.GetContext())
                    {
                        this.Sender.Tell(
                            request.ReturnUser
                                ? (object)CrudActionResponse<User>.Success(
                                    ds.Users.Include(nameof(User.Roles)).FirstOrDefault(u => u.Uid == request.UserUid),
                                    request.ExtraData)
                                : CrudActionResponse<Role>.Success(
                                    ds.Roles.Include(nameof(Role.Users)).FirstOrDefault(r => r.Uid == request.RoleUid),
                                    request.ExtraData));
                    }
                }
                catch (Exception exception)
                {
                    this.Sender.Tell(
                        request.ReturnUser
                            ? (object)CrudActionResponse<User>.Error(exception, request.ExtraData)
                            : CrudActionResponse<Role>.Error(exception, request.ExtraData));
                }
            }
        }
    }
}