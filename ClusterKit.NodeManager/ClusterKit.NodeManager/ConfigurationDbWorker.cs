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

    using Akka.Actor;
    using Akka.Event;

    using ClusterKit.Core.EF;
    using ClusterKit.Core.Ping;
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
        /// Initializes a new instance of the <see cref="ConfigurationDbWorker"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public ConfigurationDbWorker(BaseConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
            this.InitDatabase();

            this.Receive<PingMessage>(m => this.Sender.Tell(new PongMessage()));
        }

        protected override void Unhandled(object message)
        {
            Context.GetLogger().Debug("Unhandled message of type " + message.GetType().Name);
            base.Unhandled(message);
        }

        /// <summary>
        /// Checks current database connection. Updates database schema to latest version.
        /// </summary>
        private void InitDatabase()
        {
            try
            {
                var connectionString = Context.System.Settings.Config.GetString(ConfigConnectionStringPath);
                var databaseName = this.connectionManager.EscapeDatabaseName(Context.System.Settings.Config.GetString(ConfigDatabaseNamePath));
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
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}