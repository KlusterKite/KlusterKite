// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationDbWorker.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Singleton actor perfoming all node configuration related database working
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;
    using Akka.Util.Internal;

    using ClusterKit.Core.EF;
    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.Core.Utils;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;

    using JetBrains.Annotations;

    using NuGet;

    /// <summary>
    /// Singleton actor performing all node configuration related work
    /// </summary>
    [UsedImplicitly]
    public class NodeManagerActor : ReceiveActor, IWithUnboundedStash
    {
        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string ConfigConnectionStringPath = "ClusterKit.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string ConfigDatabaseNamePath = "ClusterKit.NodeManager.ConfigurationDatabaseName";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        public const string PacakgeRepositoryUrlPath = "ClusterKit.NodeManager.PackageRepository";

        /// <summary>
        /// List of node by templates
        /// </summary>
        private readonly Dictionary<string, List<Address>> activeNodesByTemplate = new Dictionary<string, List<Address>>();

        /// <summary>
        /// List of node by templates
        /// </summary>
        private readonly Dictionary<string, List<Guid>> awaitingRequestsByTemplate = new Dictionary<string, List<Guid>>();

        /// <summary>
        /// Current database connection manager
        /// </summary>
        private readonly BaseConnectionManager connectionManager;

        /// <summary>
        /// In case of cluster is full, timespan that new node candidate will repeat join request
        /// </summary>
        private readonly TimeSpan fullClusterWaitTimeout;

        /// <summary>
        /// After configuration request, it is assumed that new node of template shuld be up soon, and this is taken into account on subsequient configuration templates. This is timeout, after it it is supposed that something have gone wrong and that request is obsolete.
        /// </summary>
        private readonly TimeSpan newNodeJoinTimeout;

        /// <summary>
        /// List of known node desciptions
        /// </summary>
        private readonly Dictionary<Address, NodeDescription> nodeDescriptions = new Dictionary<Address, NodeDescription>();

        /// <summary>
        /// List of configured node templates
        /// </summary>
        private readonly Dictionary<string, NodeTemplate> nodeTemplates = new Dictionary<string, NodeTemplate>();

        /// <summary>
        /// List of configured seed nuget feeds
        /// </summary>
        private readonly Dictionary<int, NugetFeed> nugetFeeds = new Dictionary<int, NugetFeed>();

        /// <summary>
        /// List of packages in local repository;
        /// </summary>
        private readonly List<IPackage> packages = new List<IPackage>();

        /// <summary>
        /// List of pending node description requests
        /// </summary>
        private readonly Dictionary<Address, Cancelable> requestDescriptionNotifications = new Dictionary<Address, Cancelable>();

        /// <summary>
        /// List of configured seed addresses
        /// </summary>
        private readonly Dictionary<int, SeedAddress> seedAddresses = new Dictionary<int, SeedAddress>();

        /// <summary>
        /// Maximum number of <seealso cref="RequestDescriptionNotification"/> sent to newly joined node
        /// </summary>
        private int newNodeRequestDescriptionNotificationMaxRequests;

        /// <summary>
        /// Timeout to send new <seealso cref="RequestDescriptionNotification"/> message to newly joined node
        /// </summary>
        private TimeSpan newNodeRequestDescriptionNotificationTimeout;

        /// <summary>
        /// Child actor workers
        /// </summary>
        private IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActor"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public NodeManagerActor(BaseConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;

            this.fullClusterWaitTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.FullClusterWaitTimeout", TimeSpan.FromSeconds(60), false);
            this.newNodeJoinTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.NewNodeJoinTimeout", TimeSpan.FromSeconds(30), false);
            this.newNodeRequestDescriptionNotificationTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.NewNodeRequestDescriptionNotificationTimeout", TimeSpan.FromSeconds(10), false);
            this.newNodeRequestDescriptionNotificationMaxRequests = Context.System.Settings.Config.GetInt("ClusterKit.NodeManager.NewNodeRequestDescriptionNotificationMaxRequests", 10);

            this.Self.Tell(new InitializationMessage());
            this.Receive<InitializationMessage>(m => this.Initialize());
            this.Receive<object>(m => this.Stash.Stash());
            Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());
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
                Context.GetLogger().Warning("{Type}: recieved null message", this.GetType().Name);
            }
            else
            {
                Context.GetLogger().Warning("{Type}: recieved unsupported message of type {MessageTypeName}", this.GetType().Name, message.GetType().Name);
            }
            base.Unhandled(message);
        }

        /// <summary>
        /// Checks current database connection. Updates database schema to latest version.
        /// </summary>
        private void InitDatabase()
        {
            var connectionString = Context.System.Settings.Config.GetString(ConfigConnectionStringPath);
            var databaseName =
                this.connectionManager.EscapeDatabaseName(
                    Context.System.Settings.Config.GetString(ConfigDatabaseNamePath));
            using (var connection = this.connectionManager.CreateConnection(connectionString))
            {
                connection.Open();
                this.connectionManager.CheckCreateDatabase(connection, databaseName);
                connection.ChangeDatabase(databaseName);
                using (var context = new ConfigurationContext(connection))
                {
                    var migrator =
                        new MigrateDatabaseToLatestVersion
                            <ConfigurationContext, ConfigurationSource.Migrations.Configuration>(true);
                    migrator.InitializeDatabase(context);

                    context.InitEmptyTemplates();

                    if (!context.SeedAddresses.Any())
                    {
                        var seedsFromConfig =
                            Cluster.Get(Context.System)
                                .Settings.SeedNodes.Select(
                                    address =>
                                    new SeedAddress
                                    {
                                        Address =
                                                $"{address.Protocol}://{address.System}@{address.Host}:{address.Port}"
                                    });

                        foreach (var seedAddress in seedsFromConfig)
                        {
                            context.SeedAddresses.Add(seedAddress);
                        }

                        context.SaveChanges();
                    }

                    if (!context.NugetFeeds.Any())
                    {
                        var config = Context.System.Settings.Config.GetConfig(
                            "ClusterKit.NodeManager.DefaultNugetFeeds");
                        if (config != null)
                        {
                            foreach (var pair in config.AsEnumerable())
                            {
                                var feedConfig = config.GetConfig(pair.Key);

                                NugetFeed.EnFeedType feedType;
                                if (!Enum.TryParse<NugetFeed.EnFeedType>(feedConfig.GetString("type"), out feedType))
                                {
                                    feedType = NugetFeed.EnFeedType.Private;
                                }

                                context.NugetFeeds.Add(
                                    new NugetFeed { Address = feedConfig.GetString("address"), Type = feedType });
                            }

                            context.SaveChanges();
                        }
                    }

                    context.Templates.ForEach(t => this.nodeTemplates[t.Code] = t);
                    context.SeedAddresses.ForEach(s => this.seedAddresses[s.Id] = s);
                    context.NugetFeeds.ForEach(f => this.nugetFeeds[f.Id] = f);
                }
            }
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
                Context.GetLogger().Error(e, "{Type}: Exception during initialization", this.GetType().Name);
                Context.System.Scheduler.ScheduleTellOnce(
                    TimeSpan.FromSeconds(5),
                    this.Self,
                    new InitializationMessage(),
                    this.Self);
                return;
            }

            try
            {
                this.ReloadPackageList();
            }
            catch (Exception e)
            {
                Context.GetLogger().Error(e, "{Type}: Exception during package list load", this.GetType().Name);
                Context.System.Scheduler.ScheduleTellOnce(
                    TimeSpan.FromSeconds(5),
                    this.Self,
                    new InitializationMessage(),
                    this.Self);
                return;
            }

            this.workers =
                Context.ActorOf(
                    Props.Create(() => new Worker(this.connectionManager, this.Self))
                        .WithRouter(this.Self.GetFromConfiguration(Context.System, "workers")),
                    "workers");

            this.Become(this.Start);
            this.Stash.UnstashAll();
        }

        /// <summary>
        /// Receiver acror reveled itself, sending request
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
        /// There is new node going to be up and we need to decide wich template should be applied
        /// </summary>
        /// <param name="request"></param>
        private void OnNewNodeTemplateRequest(NewNodeTemplateRequest request)
        {
            var availableTmplates =
                this.nodeTemplates.Values.Where(t => t.ContainerTypes.Contains(request.ContainerType))
                    .Select(
                        t =>
                        new
                        {
                            Template = t,
                            NodesCount =
                            (this.activeNodesByTemplate.ContainsKey(t.Code) ? this.activeNodesByTemplate[t.Code].Count : 0)
                            + (this.awaitingRequestsByTemplate.ContainsKey(t.Code) ? this.awaitingRequestsByTemplate[t.Code].Count : 0)
                        })
                    .ToList();

            // first we choos among templates that have nodes less then minimum required
            var templates =
                availableTmplates.Where(
                    t => t.Template.MininmumRequiredInstances > 0 && t.NodesCount < t.Template.MininmumRequiredInstances)
                    .Select(t => t.Template)
                    .ToList();

            if (templates.Count == 0)
            {
                // if all node templates has at least minimum required node quantity, we will use node template, untill it has maximum needed quantity
                templates =
                    availableTmplates.Where(
                        t =>
                        !t.Template.MaximumNeededInstances.HasValue
                        || (t.Template.MaximumNeededInstances.Value > 0
                            && t.NodesCount < t.Template.MaximumNeededInstances.Value)).Select(t => t.Template).ToList();
            }

            if (templates.Count == 0)
            {
                // Cluster is full, we don't need any new nodes
                this.Sender.Tell(new NodeStartupWaitMessage { WaitTime = this.fullClusterWaitTimeout });
                return;
            }

            var dice = new Random().NextDouble();
            var sumWeight = templates.Sum(t => t.Priority);

            var check = 0.0;
            NodeTemplate selectedTemplate = null;
            foreach (var template in templates)
            {
                check += template.Priority / sumWeight;
                if (check <= dice)
                {
                    selectedTemplate = template;
                    break;
                }
            }

            // this could never happen, but code analyzers can't understand it
            if (selectedTemplate == null)
            {
                selectedTemplate = templates.Last();
            }

            var requestId = Guid.NewGuid();

            var rnd = new Random();

            this.Sender.Tell(
                new NodeStartUpConfiguration
                {
                    NodeTemplate = selectedTemplate.Code,
                    NodeTemplateVersion = selectedTemplate.Version,
                    Configuration = selectedTemplate.Configuration,
                    RequestId = requestId,
                    Seeds = this.seedAddresses.Values.Select(s => s.Address).OrderBy(s => rnd.NextDouble()).ToList(),
                    Packages = selectedTemplate.Packages,
                    PackageSources = this.nugetFeeds.Values.Select(f => f.Address).ToList()
                });

            List<Guid> requests;
            if (!this.awaitingRequestsByTemplate.TryGetValue(selectedTemplate.Code, out requests))
            {
                requests = new List<Guid>();
                this.awaitingRequestsByTemplate[selectedTemplate.Code] = requests;
            }

            requests.Add(requestId);

            Context.System.Scheduler.ScheduleTellOnce(
                this.newNodeJoinTimeout,
                this.Self,
                new RequestTimeOut { RequestId = requestId, TemplateCode = selectedTemplate.Code },
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
                Context.GetLogger()
                    .Warning(
                        "{Type}: received nodeDescription with null address from {NodeAddress}",
                        this.GetType().Name,
                        this.Sender.Path.Address.ToString());
                return;
            }

            if (!this.requestDescriptionNotifications.TryGetValue(address, out cancelable))
            {
                Context.GetLogger()
                    .Warning(
                        "{Type}: received nodeDescription from uknown node with address {NodeAddress}",
                        this.GetType().Name,
                        address.ToString());
                return;
            }

            cancelable.Cancel();
            this.requestDescriptionNotifications.Remove(address);
            this.nodeDescriptions[address] = nodeDescription;

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
                    awaitingRequests.Remove(nodeDescription.RequestId);
                }

                Context.GetLogger()
                    .Info(
                        "{Type}: New node {NodeTemplateName} on address {NodeAddress}",
                        this.GetType().Name,
                        nodeDescription.NodeTemplate,
                        address.ToString());
            }
            else
            {
                Context.GetLogger()
                    .Info(
                        "{Type}: New node without nodetemplate on address {NodeAddress}",
                        this.GetType().Name,
                        address.ToString());
            }
        }

        /// <summary>
        /// Processes node removed cluster event
        /// </summary>
        /// <param name="address">Obsolete node address</param>
        private void OnNodeDown(Address address)
        {
            Cancelable cancelable;
            if (this.requestDescriptionNotifications.TryGetValue(address, out cancelable))
            {
                cancelable.Cancel();
                this.requestDescriptionNotifications.Remove(address);
            }

            var nodeDescription = this.nodeDescriptions[address];
            this.nodeDescriptions.Remove(address);

            if (!string.IsNullOrWhiteSpace(nodeDescription?.NodeTemplate))
            {
                this.activeNodesByTemplate[nodeDescription.NodeTemplate].Remove(address);
            }
        }

        /// <summary>
        /// Process the node template update event
        /// </summary>
        /// <param name="message">Note template update notification</param>
        private void OnNodeTemplateUpdate(UpdateMessage<NodeTemplate> message)
        {
            switch (message.ActionType)
            {
                case EnActionType.Create:
                    this.nodeTemplates[message.NewObject.Code] = message.NewObject;
                    break;

                case EnActionType.Update:
                    this.nodeTemplates[message.NewObject.Code] = message.NewObject;
                    if (message.NewObject.Code != message.OldObject.Code)
                    {
                        this.nodeTemplates.Remove(message.OldObject.Code);
                    }
                    break;

                case EnActionType.Delete:
                    this.nodeTemplates.Remove(message.OldObject.Code);
                    break;
            }
        }

        /// <summary>
        /// Processes new node cluster event
        /// </summary>
        /// <param name="address">New node address</param>
        private void OnNodeUp(Address address)
        {
            var cancelable = new Cancelable(Context.System.Scheduler);
            this.requestDescriptionNotifications[address] = cancelable;
            this.OnRequestDescriptionNotification(new RequestDescriptionNotification { Address = address });
        }

        /// <summary>
        /// Process of manual node upgrade request
        /// </summary>
        /// <param name="request">manual node upgrade request</param>
        private void OnNodeUpdateRequest(NodeUpgradeRequest request)
        {
            if (!this.nodeDescriptions.ContainsKey(request.Address))
            {
                this.Sender.Tell(false);
            }

            Context.ActorSelection($"{request.Address}/user/NodeManager/Receiver").Tell(new ShutdownMessage(), this.Self);
            this.Sender.Tell(true);
        }

        /// <summary>
        /// Process the <seealso cref="NugetFeed"/> update event
        /// </summary>
        /// <param name="message"><seealso cref="NugetFeed"/> update notification</param>
        private void OnNugetFeedUpdate(UpdateMessage<NugetFeed> message)
        {
            switch (message.ActionType)
            {
                case EnActionType.Create:
                case EnActionType.Update:
                    this.nugetFeeds[message.NewObject.Id] = message.NewObject;
                    break;

                case EnActionType.Delete:
                    this.seedAddresses.Remove(message.OldObject.Id);
                    break;
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

            Context.ActorSelection($"{message.Address}/user/NodeManager/Receiver").Tell(new Identify("receiver"), this.Self);

            if (message.Attempt >= this.newNodeRequestDescriptionNotificationMaxRequests)
            {
                return;
            }

            var notification = new RequestDescriptionNotification
            {
                Address = message.Address,
                Attempt = message.Attempt + 1
            };

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
                requests.Remove(requestTimeOut.RequestId);
            }
        }

        /// <summary>
        /// Process the <seealso cref="SeedAddress"/> update event
        /// </summary>
        /// <param name="message"><seealso cref="SeedAddress"/> update notification</param>
        private void OnSeedAddressUpdate(UpdateMessage<SeedAddress> message)
        {
            switch (message.ActionType)
            {
                case EnActionType.Create:
                case EnActionType.Update:
                    this.seedAddresses[message.NewObject.Id] = message.NewObject;
                    break;

                case EnActionType.Delete:
                    this.seedAddresses.Remove(message.OldObject.Id);
                    break;
            }
        }

        /// <summary>
        /// Loads list of packages from repository
        /// </summary>
        private void ReloadPackageList()
        {
            var feedUrl = Context.System.Settings.Config.GetString(PacakgeRepositoryUrlPath);
            var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(feedUrl);

            var newPackages = nugetRepository.Search(string.Empty, true).Where(p => p.IsLatestVersion).ToList();
            this.packages.Clear();
            this.packages.AddRange(newPackages);
        }

        /// <summary>
        /// Initializes normal actor work
        /// </summary>
        private void Start()
        {
            Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());

            Cluster.Get(Context.System)
                               .Subscribe(
                                   this.Self,
                                   ClusterEvent.InitialStateAsEvents,
                                   new[] { typeof(ClusterEvent.MemberRemoved), typeof(ClusterEvent.MemberUp) });

            this.Receive<ClusterEvent.MemberUp>(m => this.OnNodeUp(m.Member.Address));

            this.Receive<ClusterEvent.MemberRemoved>(m => this.OnNodeDown(m.Member.Address));

            this.Receive<RequestDescriptionNotification>(m => this.OnRequestDescriptionNotification(m));
            this.Receive<NodeDescription>(m => this.OnNodeDescription(m));
            this.Receive<ActiveNodeDescriptionsRequest>(m => this.Sender.Tell(this.nodeDescriptions.Values.ToList()));
            this.Receive<ActorIdentity>(m => this.OnActorIdentity(m));
            this.Receive<NodeUpgradeRequest>(m => this.OnNodeUpdateRequest(m));

            this.Receive<UpdateMessage<NodeTemplate>>(m => this.OnNodeTemplateUpdate(m));
            this.Receive<UpdateMessage<SeedAddress>>(m => this.OnSeedAddressUpdate(m));
            this.Receive<UpdateMessage<NugetFeed>>(m => this.OnNugetFeedUpdate(m));

            this.Receive<NewNodeTemplateRequest>(m => this.OnNewNodeTemplateRequest(m));
            this.Receive<RequestTimeOut>(m => this.OnRequestTimeOut(m));

            this.Receive<PackageListRequest>(
                m =>
                this.Sender.Tell(
                    this.packages.Select(p => new PackageDescription { Id = p.Id, Version = p.Version.ToString() }).ToList()));

            this.Receive<ReloadPackageListRequest>(
                m =>
                    {
                        try
                        {
                            this.ReloadPackageList();
                            this.Sender.Tell(true);
                        }
                        catch (Exception e)
                        {
                            this.Sender.Tell(false);
                            Context.GetLogger().Error(e, "{Type}: Exception during package list load", this.GetType().Name);
                        }
                    });

            this.Receive<CollectionRequest<NodeTemplate>>(m => this.workers.Forward(m));
            this.Receive<RestActionMessage<NodeTemplate, int>>(m => this.workers.Forward(m));
            this.Receive<CollectionRequest<SeedAddress>>(m => this.workers.Forward(m));
            this.Receive<RestActionMessage<SeedAddress, int>>(m => this.workers.Forward(m));
            this.Receive<CollectionRequest<NugetFeed>>(m => this.workers.Forward(m));
            this.Receive<RestActionMessage<NugetFeed, int>>(m => this.workers.Forward(m));
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
            public int Attempt { get; set; } = 0;
        }

        /// <summary>
        /// Notification of <seealso cref="NodeStartUpConfiguration"/> request timeout.
        /// </summary>
        private class RequestTimeOut
        {
            /// <summary>
            /// Gets or sets <seealso cref="NodeStartUpConfiguration"/> request id
            /// </summary>
            public Guid RequestId { get; set; }

            /// <summary>
            /// Gets or sets code of <seealso cref="NodeTemplate"/> assigned to request
            /// </summary>
            public string TemplateCode { get; set; }
        }

        /// <summary>
        /// Child actor intended to process database requests related to <seealso cref="NodeTemplate"/>
        /// </summary>
        private class Worker : BaseCrudActorWithNotifications<ConfigurationContext>
        {
            /// <summary>
            /// Current database connection manager
            /// </summary>
            private readonly BaseConnectionManager connectionManager;

            /// <summary>
            /// Initializes a new instance of the <see cref="Worker"/> class.
            /// </summary>
            /// <param name="connectionManager">
            /// The connection manager.
            /// </param>
            /// <param name="parent">
            /// Reference to the <seealso cref="NodeManagerActor"/>
            /// </param>
            public Worker(BaseConnectionManager connectionManager, IActorRef parent) : base(parent)
            {
                Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());
                this.connectionManager = connectionManager;

                this.Receive<RestActionMessage<NodeTemplate, int>>(
                    m => this.OnRequest(m, c => c.Templates, t => t.Id, id => t => t.Id == id));
                this.Receive<RestActionMessage<SeedAddress, int>>(
                    m => this.OnRequest(m, c => c.SeedAddresses, s => s.Id, id => s => s.Id == id));
                this.Receive<RestActionMessage<NugetFeed, int>>(
                    m => this.OnRequest(m, c => c.NugetFeeds, s => s.Id, id => s => s.Id == id));

                this.Receive<CollectionRequest<NodeTemplate>>(m => this.OnCollectionRequest(m, c => c.Templates, templates => templates.OrderBy(t => t.Code)));
                this.Receive<CollectionRequest<SeedAddress>>(m => this.OnCollectionRequest(m, c => c.SeedAddresses, seeds => seeds.OrderBy(t => t.Id)));
                this.Receive<CollectionRequest<NugetFeed>>(m => this.OnCollectionRequest(m, c => c.NugetFeeds, feeds => feeds.OrderBy(t => t.Id)));
            }

            /// <summary>
            /// Method called before object modification in database
            /// </summary>
            /// <param name="newObject">The new Object.
            ///             </param><param name="oldObject">The old Object.
            ///             </param>
            /// <returns>
            /// The new version of object or null to prevent update
            /// </returns>
            protected override TObject BeforeUpdate<TObject>(TObject newObject, TObject oldObject)
            {
                if (typeof(TObject) == typeof(NodeTemplate))
                {
                    ((NodeTemplate)(object)newObject).Version = ((NodeTemplate)(object)oldObject).Version + 1;
                }

                // newObject.Version = oldObject.Version + 1;
                return base.BeforeUpdate(newObject, oldObject);
            }

            /// <summary>
            /// Opens new database connection and generates execution context
            /// </summary>
            /// <returns>New working context</returns>
            protected override async Task<ConfigurationContext> GetContext()
            {
                var connectionString = Context.System.Settings.Config.GetString(ConfigConnectionStringPath);
                var databaseName =
                    this.connectionManager.EscapeDatabaseName(
                        Context.System.Settings.Config.GetString(ConfigDatabaseNamePath));
                var connection = this.connectionManager.CreateConnection(connectionString);
                await connection.OpenAsync();
                connection.ChangeDatabase(databaseName);
                return new ConfigurationContext(connection);
            }
        }
    }
}