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
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Cluster;
    using Akka.Event;
    using Akka.Routing;

    using ClusterKit.Core.EF;
    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.Core.Utils;
    using ClusterKit.NodeManager.Client.Messages;
    using ClusterKit.NodeManager.ConfigurationSource;
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
        /// Current database connection manager
        /// </summary>
        private readonly BaseConnectionManager connectionManager;

        /// <summary>
        /// List of known node desciptions
        /// </summary>
        private readonly Dictionary<Address, NodeDescription> nodeDescriptions = new Dictionary<Address, NodeDescription>();

        /// <summary>
        /// List of pending node description requests
        /// </summary>
        private readonly Dictionary<Address, Cancelable> requestDescriptionNotifications = new Dictionary<Address, Cancelable>();

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
            Context.GetLogger().Warning("{Type}: recieved unsupported message of type {MessageTypeName}", this.GetType().Name, message.GetType().Name);
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
                        new MigrateDatabaseToLatestVersion<ConfigurationContext, ConfigurationSource.Migrations.Configuration>(true);
                    migrator.InitializeDatabase(context);

                    context.InitEmptyTemplates();
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
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(5), this.Self, new InitializationMessage(), this.Self);
                return;
            }

            this.workers =
                Context.ActorOf(
                    Props.Create(() => new Worker(this.connectionManager))
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
        /// On new node description
        /// </summary>
        /// <param name="nodeDescription">Node description</param>
        private void OnNodeDescription(NodeDescription nodeDescription)
        {
            Cancelable cancelable;
            var address = nodeDescription.NodeAddress;
            if (!this.requestDescriptionNotifications.TryGetValue(address, out cancelable))
            {
                Context.GetLogger().Warning(
                    "{Type}: received nodeDescription from uknown node with address {NodeAddress}",
                    this.GetType().Name,
                    address.ToString());
                return;
            }

            cancelable.Cancel();
            this.requestDescriptionNotifications.Remove(address);
            this.nodeDescriptions[address] = nodeDescription;

            Context.GetLogger().Warning(
                    "{Type}: New node {NodeTemplateName} on address {NodeAddress}",
                    this.GetType().Name,
                    nodeDescription.NodeTemplate,
                    address.ToString());
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

            this.nodeDescriptions.Remove(address);
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
            Context.System.Scheduler.ScheduleTellOnce(
                TimeSpan.FromSeconds(10), // todo: @kantora move to configuration
                this.Self,
                message,
                this.Self,
                cancelable);
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

            this.Receive<ClusterEvent.MemberUp>(
                m => m.Member.Roles.Contains("Web.Swagger.Publish"),
                m => this.OnNodeUp(m.Member.Address));

            this.Receive<ClusterEvent.MemberRemoved>(
                m => m.Member.Roles.Contains("Web.Swagger.Publish"),
                m => this.OnNodeDown(m.Member.Address));

            this.Receive<RequestDescriptionNotification>(m => this.OnRequestDescriptionNotification(m));
            this.Receive<NodeDescription>(m => this.OnNodeDescription(m));
            this.Receive<ActiveNodeDescriptionsRequest>(m => this.Sender.Tell(this.nodeDescriptions.Values.ToList()));
            this.Receive<ActorIdentity>(m => this.OnActorIdentity(m));

            this.Receive<IConsistentHashable>(m => this.workers.Forward(m));
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
            /// Address of the node
            /// </summary>
            public Address Address { get; set; }
        }

        /// <summary>
        /// Child actor intended to process database requests
        /// </summary>
        private class Worker : BaseCrudActor<ConfigurationContext, NodeTemplate, int>
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
            public Worker(BaseConnectionManager connectionManager)
            {
                Context.GetLogger().Info("{Type}: started on {Path}", this.GetType().Name, this.Self.Path.ToString());
                this.connectionManager = connectionManager;
                this.Receive<CollectionRequest>(m => this.OnCollectionRequest(m));
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
            protected override NodeTemplate BeforeUpdate(NodeTemplate newObject, NodeTemplate oldObject)
            {
                newObject.Version = oldObject.Version + 1;
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

            /// <summary>
            /// Gets the table from context, corresponding current data object
            /// </summary>
            /// <param name="context">The context.
            ///             </param>
            /// <returns>
            /// The table
            /// </returns>
            protected override DbSet<NodeTemplate> GetDbSet(ConfigurationContext context) => context.Templates;

            /// <summary>
            /// Gets an object id
            /// </summary>
            /// <param name="object">The data object</param>
            /// <returns>
            /// Object identification number
            /// </returns>
            protected override int GetId(NodeTemplate @object) => @object.Id;

            /// <summary>
            /// Gets expression to validate id in data object
            /// </summary>
            /// <param name="id">Object identification number</param>
            /// <returns>
            /// Validity of identification field
            /// </returns>
            protected override Expression<Func<NodeTemplate, bool>> GetIdValidationExpression(int id) => o => o.Id == id;

            /// <summary>
            /// Process collection requests
            /// </summary>
            /// <param name="collectionRequest">Collection request</param>
            /// <returns>The list of objects</returns>
            private async Task OnCollectionRequest(CollectionRequest collectionRequest)
            {
                using (var ds = await this.GetContext())
                {
                    var query = ds.Templates.OrderBy(t => t.Code).Skip(collectionRequest.Skip);
                    if (collectionRequest.Count.HasValue)
                    {
                        query = query.Take(collectionRequest.Count.Value);
                    }

                    this.Sender.Tell(await query.ToListAsync());
                }
            }
        }
    }
}