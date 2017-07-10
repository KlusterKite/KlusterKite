// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseDatabaseMigrator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The base class for Entity Framework Code-first migrations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Migrator.EF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KlusterKite.Data.EF;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;

    /// <summary>
    /// The base class for Entity Framework Code-first migrations
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    public abstract class BaseDatabaseMigrator<TContext> : IMigrator
        where TContext : DbContext
    {
        /// <summary>
        /// The context factory
        /// </summary>
        private readonly UniversalContextFactory contextFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDatabaseMigrator{TContext}"/> class.
        /// </summary>
        /// <param name="contextFactory">
        /// The context factory manager.
        /// </param>
        protected BaseDatabaseMigrator(UniversalContextFactory contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        /// <inheritdoc />
        public string LatestPoint
        {
            get
            {
                var defaultResourceId = this.GetDefaultResourceId();
                using (var context = this.contextFactory.CreateContext<TContext>(
                    defaultResourceId.ProviderName,
                    defaultResourceId.ConnectionString))
                {
                    return context.Database.GetMigrations().OrderBy(m => m).Last();
                }
            }
        }

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract IEnumerable<ResourceId> GetMigratableResources();

        /// <summary>
        /// The default resource id to check overall migration parameters
        /// </summary>
        /// <returns>The default resource id</returns>
        public abstract ResourceId GetDefaultResourceId();

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            using (var context =
                this.contextFactory.CreateContext<TContext>(resourceId.ProviderName, resourceId.ConnectionString))
            {
                return context.Database.GetAppliedMigrations().OrderBy(t => t).Last();
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> Migrate(ResourceId resourceId, string pointToMigrate)
        {
            using (var context =
                this.contextFactory.CreateContext<TContext>(resourceId.ProviderName, resourceId.ConnectionString))
            {
                yield return $"Updating database on connection {resourceId.ConnectionString} to {pointToMigrate}";
                context.Database.Migrate();
                var databaseMigrator = context.Database.GetService<Microsoft.EntityFrameworkCore.Migrations.IMigrator>();
                databaseMigrator.Migrate(pointToMigrate);
            }

            using (var context =
                this.contextFactory.CreateContext<TContext>(resourceId.ProviderName, resourceId.ConnectionString))
            {
                var point = context.Database.GetAppliedMigrations().OrderBy(t => t).Last();
                if (point != pointToMigrate)
                {
                    throw new Exception("Database migration was ignored");
                }

                yield return $"Database on connection {resourceId.ConnectionString} to {pointToMigrate} was updated";
            }
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllPoints()
        {
            var defaultResourceId = this.GetDefaultResourceId();
            using (var context = this.contextFactory.CreateContext<TContext>(
                defaultResourceId.ProviderName,
                defaultResourceId.ConnectionString))
            {
                return context.Database.GetMigrations().OrderBy(m => m);
            }
        }
    }
}
