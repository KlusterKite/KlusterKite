﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;
    using Akka.Util.Internal;

    using ClusterKit.Core;
    using ClusterKit.Core.Data;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.Core.Utils;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Messages;

    using JetBrains.Annotations;

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
        /// The data source context factory
        /// </summary>
        private readonly IContextFactory<ConfigurationContext> contextFactory;

        /// <summary>
        /// In case of cluster is full, timespan that new node candidate will repeat join request
        /// </summary>
        private readonly TimeSpan fullClusterWaitTimeout;

        /// <summary>
        /// After configuration request, it is assumed that new node of template shuld be up soon, and this is taken into account on subsequient configuration templates. This is timeout, after it it is supposed that something have gone wrong and that request is obsolete.
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
        /// List of pending node description requests
        /// </summary>
        private readonly Dictionary<Address, Cancelable> requestDescriptionNotifications = new Dictionary<Address, Cancelable>();

        /// <summary>
        /// The node message router
        /// </summary>
        private readonly IMessageRouter router;

        /// <summary>
        /// List of configured seed addresses
        /// </summary>
        private readonly Dictionary<int, SeedAddress> seedAddresses = new Dictionary<int, SeedAddress>();

        /// <summary>
        /// Part of currently active nodes, that could be sent to upgrade at once. 1.0M - all active nodes (above minimum required) could be sent to upgrade.
        /// </summary>
        private readonly decimal upgradablePart;

        /// <summary>
        /// List of nodes with upgrade in progress
        /// </summary>
        private readonly Dictionary<Guid, UpgradeData> upgradingNodes = new Dictionary<Guid, UpgradeData>();

        /// <summary>
        /// The database connection string
        /// </summary>
        private string connectionString;

        /// <summary>
        /// The database name
        /// </summary>
        private string databaseName;

        /// <summary>
        /// List of packages in local repository;
        /// </summary>
        private Dictionary<string, PackageDescription> packages = new Dictionary<string, PackageDescription>();

        private Cancelable upgradeMessageSchedule;

        /// <summary>
        /// Child actor workers
        /// </summary>
        private IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="NodeManagerActor"/> class.
        /// </summary>
        /// <param name="contextFactory">
        /// Configuration context factory
        /// </param>
        /// <param name="router">
        /// The node message router
        /// </param>
        public NodeManagerActor(
            IContextFactory<ConfigurationContext> contextFactory,
            IMessageRouter router)
        {
            this.contextFactory = contextFactory;
            this.router = router;

            this.fullClusterWaitTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.FullClusterWaitTimeout", TimeSpan.FromSeconds(60), false);
            this.newNodeJoinTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.NewNodeJoinTimeout", TimeSpan.FromSeconds(30), false);
            this.newNodeRequestDescriptionNotificationTimeout = Context.System.Settings.Config.GetTimeSpan("ClusterKit.NodeManager.NewNodeRequestDescriptionNotificationTimeout", TimeSpan.FromSeconds(10), false);
            this.newNodeRequestDescriptionNotificationMaxRequests = Context.System.Settings.Config.GetInt("ClusterKit.NodeManager.NewNodeRequestDescriptionNotificationMaxRequests", 10);
            this.upgradablePart = Context.System.Settings.Config.GetDecimal("ClusterKit.NodeManager.NewNodeRequestDescriptionNotificationMaxRequests", 10);

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
        /// Checks node for available upgrades
        /// </summary>
        /// <param name="nodeDescription">Node description to check</param>
        private void CheckNodeIsObsolete(NodeDescription nodeDescription)
        {
            nodeDescription.IsObsolete = false;
            NodeTemplate template;
            if (!this.nodeTemplates.TryGetValue(nodeDescription.NodeTemplate, out template)
                || template.Version != nodeDescription.NodeTemplateVersion)
            {
                nodeDescription.IsObsolete = true;
            }

            foreach (var module in nodeDescription.Modules)
            {
                PackageDescription package;
                if (!this.packages.TryGetValue(module.Id, out package))
                {
                    Context.GetLogger().Error(
                        "{Type}: node with template {TemplateCode} on container {ContainerCode} on address {NodeAddressString} has module {PackageId} that does not contained in repository. This node cannot be upgraded",
                        this.GetType().Name,
                        nodeDescription.NodeTemplate,
                        nodeDescription.ContainerType,
                        nodeDescription.NodeAddress.ToString(),
                        module.Id);
                    nodeDescription.IsObsolete = false;
                    return;
                }

                if (package.Version != module.Version)
                {
                    nodeDescription.IsObsolete = true;
                }
            }
        }

        /// <summary>
        /// Checks current database connection. Updates database schema to latest version.
        /// </summary>
        private void InitDatabase()
        {
            this.connectionString = Context.System.Settings.Config.GetString(ConfigConnectionStringPath);
            this.databaseName = Context.System.Settings.Config.GetString(ConfigDatabaseNamePath);

            using (var context = this.contextFactory.CreateAndUpgradeContext(this.connectionString, this.databaseName).Result)
            {
                DataFactory<ConfigurationContext, NodeTemplate, int>
                    .CreateFactory(context)
                    .GetList(0, null).Result
                    .ForEach(t => this.nodeTemplates[t.Code] = t);

                DataFactory<ConfigurationContext, SeedAddress, int>
                    .CreateFactory(context)
                    .GetList(0, null).Result
                    .ForEach(s => this.seedAddresses[s.Id] = s);

                DataFactory<ConfigurationContext, NugetFeed, int>
                    .CreateFactory(context)
                    .GetList(0, null).Result
                    .ForEach(f => this.nugetFeeds[f.Id] = f);
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
                    Props.Create(() => new Worker(this.connectionString, this.databaseName, this.contextFactory, this.Self))
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

            var rnd = new Random();

            this.Sender.Tell(
                new NodeStartUpConfiguration
                {
                    NodeTemplate = selectedTemplate.Code,
                    NodeTemplateVersion = selectedTemplate.Version,
                    Configuration = selectedTemplate.Configuration,
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

            requests.Add(request.NodeUid);

            Context.System.Scheduler.ScheduleTellOnce(
                this.newNodeJoinTimeout,
                this.Self,
                new RequestTimeOut { NodeId = request.NodeUid, TemplateCode = selectedTemplate.Code },
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

            this.upgradingNodes.Remove(nodeDescription.NodeId);
            this.CheckNodeIsObsolete(nodeDescription);

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
                    awaitingRequests.Remove(nodeDescription.NodeId);
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

            this.Self.Tell(new UpgradeMessage());
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

                    foreach (var value in this.nodeDescriptions.Values)
                    {
                        this.CheckNodeIsObsolete(value);
                    }

                    this.OnNodeUpgrade();
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
        /// <param name="address">Address of the node tu upgrade</param>
        private void OnNodeUpdateRequest(Address address)
        {
            if (!this.nodeDescriptions.ContainsKey(address))
            {
                this.Sender.Tell(false);
            }

            this.router.Tell(address, "/user/NodeManager/Receiver", new ShutdownMessage(), this.Self);
            this.Sender.Tell(true);
        }

        /// <summary>
        /// Checks current nodes for possible upgrade need and performs an upgrade
        /// </summary>
        private void OnNodeUpgrade()
        {
            this.upgradeMessageSchedule?.Cancel();

            var upgradeTimeOut = this.newNodeJoinTimeout + this.newNodeRequestDescriptionNotificationTimeout;
            // removing lost nodes
            var obsoleteUpgrades =
                this.upgradingNodes.Values.Where(
                    u => DateTimeOffset.Now - u.UpgradeStartTime >= upgradeTimeOut).ToList();

            obsoleteUpgrades.ForEach(u => this.upgradingNodes.Remove(u.NodeId));
            var isUpgrading = false;

            // searching for nodes to upgrade
            var nodesToUpgrade = this.nodeDescriptions.Values.Where(n => n.IsObsolete).GroupBy(n => n.NodeTemplate);
            foreach (var nodeGroup in nodesToUpgrade)
            {
                NodeTemplate nodeTemplate;
                if (!this.nodeTemplates.TryGetValue(nodeGroup.Key, out nodeTemplate))
                {
                    Context.GetLogger()
                        .Error(
                            "{Type}: could not find template with {TemplateCode} during upgrade proccess",
                            this.GetType().Name,
                            nodeGroup.Key);
                    nodeGroup.ForEach(n => n.IsObsolete = false);
                    continue;
                }

                if (nodeGroup.Count() <= nodeTemplate.MininmumRequiredInstances)
                {
                    // node upgrade is blocked if it will cause cluster mulfunction
                    continue;
                }

                var nodesInUpgrade = this.upgradingNodes.Values.Count(u => u.NodeTemplate == nodeGroup.Key);

                var nodesToUpgradeCount = (int)Math.Ceiling(nodeGroup.Count() * 100.0M / this.upgradablePart)
                                          - nodesInUpgrade;

                if (nodesToUpgradeCount <= 0)
                {
                    continue;
                }

                var nodes = nodeGroup.OrderBy(n => n.StartTimeStamp).Take(nodesToUpgradeCount);
                isUpgrading = true;
                foreach (var node in nodes)
                {
                    this.upgradingNodes[node.NodeId] = new UpgradeData
                    {
                        NodeId = node.NodeId,
                        NodeTemplate = node.NodeTemplate,
                        UpgradeStartTime = DateTimeOffset.Now
                    };
                    this.OnNodeUpdateRequest(node.NodeAddress);
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

            this.router.Tell(message.Address, "/user/NodeManager/Receiver", new Identify("receiver"), this.Self);

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
                requests.Remove(requestTimeOut.NodeId);
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
            var factory = DataFactory<string, PackageDescription, string>.CreateFactory(feedUrl);

            this.packages = factory.GetList(0, null).Result.ToDictionary(p => p.Id);

            foreach (var node in this.nodeDescriptions.Values)
            {
                this.CheckNodeIsObsolete(node);
            }

            this.Self.Tell(new UpgradeMessage());
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

            // ping message will indicate that actor started and ready to work
            this.Receive<PingMessage>(m => this.Sender.Tell(new PongMessage()));

            this.Receive<ClusterEvent.MemberUp>(m => this.OnNodeUp(m.Member.Address));
            this.Receive<ClusterEvent.MemberRemoved>(m => this.OnNodeDown(m.Member.Address));

            this.Receive<RequestDescriptionNotification>(m => this.OnRequestDescriptionNotification(m));
            this.Receive<NodeDescription>(m => this.OnNodeDescription(m));
            this.Receive<ActiveNodeDescriptionsRequest>(
                m =>
                    {
                        this.Sender.Tell(this.nodeDescriptions.Values.ToList());
                    });
            this.Receive<ActorIdentity>(m => this.OnActorIdentity(m));
            this.Receive<NodeUpgradeRequest>(m => this.OnNodeUpdateRequest(m.Address));

            this.Receive<UpdateMessage<NodeTemplate>>(m => this.OnNodeTemplateUpdate(m));
            this.Receive<UpdateMessage<SeedAddress>>(m => this.OnSeedAddressUpdate(m));
            this.Receive<UpdateMessage<NugetFeed>>(m => this.OnNugetFeedUpdate(m));

            this.Receive<NewNodeTemplateRequest>(m => this.OnNewNodeTemplateRequest(m));
            this.Receive<RequestTimeOut>(m => this.OnRequestTimeOut(m));

            this.Receive<PackageListRequest>(m => this.Sender.Tell(this.packages.Values.ToList()));

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

            this.Receive<UpgradeMessage>(m => this.OnNodeUpgrade());

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
            /// Gets or sets time of upgrade initialization proccess
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
            private readonly string connectionString;

            /// <summary>
            /// Configuration context factory
            /// </summary>
            private readonly IContextFactory<ConfigurationContext> contextFactory;

            private readonly string databaseName;

            /// <summary>
            /// Initializes a new instance of the <see cref="Worker"/> class.
            /// </summary>
            /// <param name="connectionString">
            /// The database connection string
            /// </param>
            /// <param name="databaseName">
            /// The database name
            /// </param>
            /// <param name="contextFactory">
            /// Configuration context factory
            /// </param>
            /// <param name="parent">
            /// Reference to the <seealso cref="NodeManagerActor"/>
            /// </param>
            public Worker(
                string connectionString,
                string databaseName,
                IContextFactory<ConfigurationContext> contextFactory,
                IActorRef parent) : base(parent)
            {
                Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());

                this.connectionString = connectionString;
                this.databaseName = databaseName;
                this.contextFactory = contextFactory;

                this.ReceiveAsync<RestActionMessage<NodeTemplate, int>>(
                    this.OnRequest);
                this.ReceiveAsync<RestActionMessage<SeedAddress, int>>(
                    this.OnRequest);
                this.ReceiveAsync<RestActionMessage<NugetFeed, int>>(
                    this.OnRequest);

                this.ReceiveAsync<CollectionRequest<NodeTemplate>>(this.OnCollectionRequest<NodeTemplate, int>);
                this.ReceiveAsync<CollectionRequest<SeedAddress>>(this.OnCollectionRequest<SeedAddress, int>);
                this.ReceiveAsync<CollectionRequest<NugetFeed>>(this.OnCollectionRequest<NugetFeed, int>);
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

                return base.BeforeUpdate(newObject, oldObject);
            }

            /// <summary>
            /// Opens new database connection and generates execution context
            /// </summary>
            /// <returns>New working context</returns>
            protected override async Task<ConfigurationContext> GetContext()
            {
                return await this.contextFactory.CreateContext(this.connectionString, this.databaseName);
            }
        }
    }
}