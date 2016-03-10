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
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;
    using Akka.Routing;

    using ClusterKit.Core.EF;
    using ClusterKit.Core.Ping;
    using ClusterKit.Core.Rest.ActionMessages;
    using ClusterKit.Core.Utils;
    using ClusterKit.NodeManager.ConfigurationSource;

    /// <summary>
    /// Singleton actor performing all node configuration related database working
    /// </summary>
    public class ConfigurationDbWorker : ReceiveActor
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
        /// Child actor workers
        /// </summary>
        private IActorRef workers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDbWorker"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public ConfigurationDbWorker(BaseConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            this.InitDatabase();

            this.workers =
                Context.ActorOf(
                    Props.Create(() => new Worker(connectionManager))
                        .WithRouter(this.Self.GetFromConfiguration(Context.System, "workers")),
                    "workers");

            this.Receive<IConsistentHashable>(m => this.workers.Forward(m));
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
                }
            }
        }

        /// <summary>
        /// Child actor intended to process database requests
        /// </summary>
        protected class Worker : ReceiveActor
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
                this.connectionManager = connectionManager;
                this.Receive<RestActionMessage<NodeTemplate, int>>(m => this.OnIdRequest(m));
                this.Receive<RestActionMessage<NodeTemplate, string>>(m => this.OnCodeRequest(m));
                this.Receive<CollectionRequest>(m => this.OnCollectionRequest(m));
            }

            /// <summary>
            /// Opens new database connection and generates execution context
            /// </summary>
            /// <returns>New working context</returns>
            private async Task<ConfigurationContext> GetContext()
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
            /// Process rest request based on Code field
            /// </summary>
            /// <param name="request">Request to process</param>
            /// <returns>Executing task</returns>
            private async Task OnCodeRequest(RestActionMessage<NodeTemplate, string> request)
            {
                using (var ds = await this.GetContext())
                {
                    switch (request.ActionType)
                    {
                        case EnActionType.Get:
                            this.Sender.Tell(await ds.Templates.FirstOrDefaultAsync(n => n.Code == request.Id));
                            break;

                        case EnActionType.Create:
                        case EnActionType.Update:
                            await this.OnCreateUpdateRequest(request.ActionType, request.Request);
                            break;

                        case EnActionType.Delete:
                            ds.Templates.RemoveRange(await ds.Templates.Where(t => t.Code == request.Id).ToListAsync());
                            this.Sender.Tell(await ds.SaveChangesAsync() > 0);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            /// <summary>
            /// Process collection requests
            /// </summary>
            /// <param name="collectionRequest">Collection request</param>
            /// <returns>The list of objects</returns>
            private async Task OnCollectionRequest(CollectionRequest collectionRequest)
            {
                using (var ds = await this.GetContext())
                {
                    var query = ds.Templates.Skip(collectionRequest.Skip);
                    if (collectionRequest.Count.HasValue)
                    {
                        query = query.Take(collectionRequest.Count.Value);
                    }

                    this.Sender.Tell(await query.ToListAsync());
                }
            }

            /// <summary>
            /// Process of create or update request
            /// </summary>
            /// <param name="actionType">Action to validate</param>
            /// <param name="request">new data</param>
            /// <returns>Execution task</returns>
            private async Task OnCreateUpdateRequest(EnActionType actionType, NodeTemplate request)
            {
                using (var ds = await this.GetContext())
                {
                    if (actionType == EnActionType.Create && request.Id != 0)
                    {
                        this.Sender.Tell(null);
                        return;
                    }

                    if (actionType == EnActionType.Update)
                    {
                        if (request.Id == 0)
                        {
                            this.Sender.Tell(null);
                            return;
                        }

                        if (await ds.Templates.FirstOrDefaultAsync(t => t.Id == request.Id) == null)
                        {
                            this.Sender.Tell(null);
                            return;
                        }
                    }

                    if (await ds.Templates.FirstOrDefaultAsync(t => t.Id != request.Id && t.Code == request.Code) != null)
                    {
                        this.Sender.Tell(null);
                        return;
                    }

                    ds.Templates.Attach(request);

                    try
                    {
                        await ds.SaveChangesAsync();
                        this.Sender.Tell(request);
                    }
                    catch (Exception)
                    {
                        this.Sender.Tell(null);
                        throw;
                    }
                }
            }

            /// <summary>
            /// Process rest request based on Id field
            /// </summary>
            /// <param name="request">Request to process</param>
            /// <returns>Executing task</returns>
            private async Task OnIdRequest(RestActionMessage<NodeTemplate, int> request)
            {
                using (var ds = await this.GetContext())
                {
                    switch (request.ActionType)
                    {
                        case EnActionType.Get:
                            this.Sender.Tell(await ds.Templates.FirstOrDefaultAsync(n => n.Id == request.Id));
                            break;

                        case EnActionType.Create:
                        case EnActionType.Update:
                            await this.OnCreateUpdateRequest(request.ActionType, request.Request);
                            break;

                        case EnActionType.Delete:
                            ds.Templates.RemoveRange(await ds.Templates.Where(t => t.Id == request.Id).ToListAsync());
                            this.Sender.Tell(await ds.SaveChangesAsync() > 0);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}