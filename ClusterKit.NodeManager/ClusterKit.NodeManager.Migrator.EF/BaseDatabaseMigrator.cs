// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseDatabaseMigrator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The base class for Entity Framework Code-first migrations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Migrator.EF
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using ClusterKit.Data.EF;

    /// <summary>
    /// The base class for Entity Framework Code-first migrations
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    /// <typeparam name="TMigrationConfiguration">
    /// Database migration configuration
    /// </typeparam>
    public abstract class BaseDatabaseMigrator<TContext, TMigrationConfiguration> : IMigrator
        where TMigrationConfiguration : DbMigrationsConfiguration<TContext>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// The connection manager
        /// </summary>
        private readonly BaseConnectionManager connectionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDatabaseMigrator{TContext,TMigrationConfiguration}"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        protected BaseDatabaseMigrator(BaseConnectionManager connectionManager)
        {
            this.connectionManager = connectionManager;
        }

        /// <inheritdoc />
        public string LatestPoint => new DbMigrator(new TMigrationConfiguration()).GetLocalMigrations().Last();

        /// <inheritdoc />
        public string Name => "ClusterKit configuration database";

        /// <inheritdoc />
        public abstract IEnumerable<ResourceId> GetMigratableResources();

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            var configuration = new TMigrationConfiguration
                                    {
                                        TargetDatabase = new DbConnectionInfo(
                                            resourceId.ConnectionString,
                                            this.connectionManager.ProviderInvariantName)
                                    };
            var migrator = new DbMigrator(configuration);
            return migrator.GetDatabaseMigrations().OrderBy(t => t).Last();
        }

        /// <inheritdoc />
        public IEnumerable<string> Migrate(ResourceId resourceId, string pointToMigrate)
        {
            var configuration = new TMigrationConfiguration
                                    {
                                        TargetDatabase = new DbConnectionInfo(
                                            resourceId.ConnectionString,
                                            this.connectionManager.ProviderInvariantName)
                                    };
            var migrator = new DbMigrator(configuration);
            yield return $"Updating database on connection {resourceId.ConnectionString} to {pointToMigrate}";
            migrator.Update(pointToMigrate);
            var point = migrator.GetDatabaseMigrations().OrderBy(t => t).Last();
            if (point != pointToMigrate)
            {
                throw new Exception("Database migration was ignored");
            }

            yield return $"Database on connection {resourceId.ConnectionString} to {pointToMigrate} was updated";
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllPoints()
        {
            return new DbMigrator(new TMigrationConfiguration()).GetLocalMigrations().OrderBy(p => p);
        }
    }
}
