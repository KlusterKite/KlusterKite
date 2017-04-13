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
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Migrations;
    using System.Linq;

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
        /// <inheritdoc />
        public string LatestPoint => new DbMigrator(new TMigrationConfiguration()).GetLocalMigrations().Last();

        /// <inheritdoc />
        public abstract IEnumerable<ResourceId> GetMigratableResources();

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            var configuration =
                new TMigrationConfiguration { TargetDatabase = new DbConnectionInfo(resourceId.ConnectionString) };
            var migrator = new DbMigrator(configuration);
            return migrator.GetDatabaseMigrations().Last();
        }

        /// <inheritdoc />
        public void Migrate(ResourceId resourceId, string pointToMigrate)
        {
            var configuration =
                new TMigrationConfiguration { TargetDatabase = new DbConnectionInfo(resourceId.ConnectionString) };
            var migrator = new DbMigrator(configuration);
            migrator.Update(pointToMigrate);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllPoints()
        {
            return new DbMigrator(new TMigrationConfiguration()).GetLocalMigrations();
        }
    }
}
