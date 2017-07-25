// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MockResourceMigrator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Mock resource to test system migrations
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Mock
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Configuration;

    using KlusterKite.NodeManager.Migrator;

    using StackExchange.Redis;

    /// <summary>
    /// Mock resource to test system migrations
    /// </summary>
    public abstract class MockResourceMigrator : IMigrator
    {
        /// <summary>
        /// The migrator config
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// The redis connection string
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MockResourceMigrator"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        public MockResourceMigrator(Config config)
        {
            this.config = config;
            this.connectionString = config.GetString("KlusterKite.NodeManager.Mock.RedisConnection");
        }

        /// <inheritdoc />
        public string LatestPoint => this.GetAllPoints().Last();

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public abstract EnResourceDependencyType DependencyType { get; }

        /// <inheritdoc />
        public decimal Priority => 1M;

        /// <summary>
        /// Gets the config path to the resource points
        /// </summary>
        public abstract string ResourcePointsConfigPath { get;  }

        /// <summary>
        /// Gets the config path to the resources names
        /// </summary>
        public abstract string ResourcesConfigPath { get; }

        /// <inheritdoc />
        public IEnumerable<string> GetAllPoints()
        {
            return this.config.GetStringList(this.ResourcePointsConfigPath);
        }

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            using (var connection = ConnectionMultiplexer.Connect(this.connectionString))
            {
                var db = connection.GetDatabase();
                return db.StringGet(this.GetRedisKey(resourceId));
            }
        }

        /// <inheritdoc />
        public IEnumerable<ResourceId> GetMigratableResources()
        {
            return this.config.GetStringList(this.ResourcesConfigPath)?.Select(
                s => new ResourceId { Code = s, ConnectionString = s, Name = this.GetResourceName(s), ProviderName = "Mock" });
        }

        /// <inheritdoc />
        public IEnumerable<string> Migrate(ResourceId resourceId, string pointToMigrate)
        {
            using (var connection = ConnectionMultiplexer.Connect(this.connectionString))
            {
                var db = connection.GetDatabase();
                if (!db.StringSet(this.GetRedisKey(resourceId), pointToMigrate))
                {
                    throw new Exception($"Failed to migrate resource on connection {resourceId.ConnectionString} to {pointToMigrate}");
                }

                yield return $"Resource on connection {resourceId.ConnectionString} to {pointToMigrate} was updated";
            }
        }

        /// <summary>
        /// Creates the resource name for the resource code
        /// </summary>
        /// <param name="resourceCode">The resource code</param>
        /// <returns>The resource name</returns>
        protected abstract string GetResourceName(string resourceCode);

        /// <summary>
        /// Gets the redis key for specified resource
        /// </summary>
        /// <param name="resourceId">The resource</param>
        /// <returns>the key in Redis</returns>
        protected abstract string GetRedisKey(ResourceId resourceId);
    }
}
