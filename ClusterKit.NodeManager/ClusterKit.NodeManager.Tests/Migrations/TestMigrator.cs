// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMigrator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the TestMigrator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Akka.Configuration;

    using ClusterKit.NodeManager.Migrator;

    using JetBrains.Annotations;

    /// <summary>
    /// The test migrator
    /// </summary>
    [UsedImplicitly]
    public class TestMigrator : IMigrator
    {
        /// <summary>
        /// The migrator config
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// The list of defined migration points
        /// </summary>
        private readonly List<string> definedMigrationPoints;

        /// <summary>
        /// The list of defined migration resources
        /// </summary>
        private readonly List<string> resources;

        /// <inheritdoc />
        public TestMigrator(Config config)
        {
            this.config = config;
            this.definedMigrationPoints = config.GetStringList("TestMigrator.DefinedMigrationPoints").ToList();
            this.resources = config.GetStringList("TestMigrator.Resources").ToList();
            this.LatestPoint = this.definedMigrationPoints.Last();
        }

        /// <inheritdoc />
        public string LatestPoint { get; }

        /// <inheritdoc />
        public string Name => "Test migrator";

        /// <summary>
        /// Sets the current migration point for the resource
        /// </summary>
        /// <param name="connectionString">The resource connection string</param>
        /// <param name="point">The current migration point</param>
        public static void SetMigrationPoint(string connectionString, string point)
        {
            File.WriteAllText(connectionString, point, Encoding.UTF8);
        }

        /// <inheritdoc />
        public IEnumerable<string> GetAllPoints()
        {
            if (this.config.GetBoolean("TestMigrator.ThrowOnGetAllPoints"))
            {
                throw new Exception("GetAllPoints failed");
            }

            return this.definedMigrationPoints;
        }

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            if (this.config.GetBoolean("TestMigrator.ThrowOnGetCurrentPoint"))
            {
                throw new Exception("GetCurrentPoint failed");
            }

            if (!this.resources.Contains(resourceId.ConnectionString))
            {
                throw new InvalidOperationException($"Unknown connection {resourceId.ConnectionString}");
            }

            return File.Exists(resourceId.ConnectionString)
                       ? File.ReadAllText(resourceId.ConnectionString)
                       : this.definedMigrationPoints.Last();
        }

        /// <inheritdoc />
        public IEnumerable<ResourceId> GetMigratableResources()
        {
            if (this.config.GetBoolean("TestMigrator.ThrowOnGetMigratableResources"))
            {
                throw new Exception("GetMigratableResources failed");
            }

            foreach (var resource in this.resources)
            {
                yield return new ResourceId
                                 {
                                     ConnectionString = resource,
                                     Name = Path.GetFileName(resource),
                                     Code = Path.GetFileName(resource)
                                 };
            }
        }

        /// <inheritdoc />
        public void Migrate(ResourceId resourceId, string pointToMigrate)
        {
            if (this.config.GetBoolean("TestMigrator.ThrowOnMigrate"))
            {
                throw new Exception("Migrate failed");
            }

            if (!this.resources.Contains(resourceId.ConnectionString))
            {
                throw new InvalidOperationException($"Unknown connection {resourceId.ConnectionString}");
            }

            if (!this.definedMigrationPoints.Contains(pointToMigrate))
            {
                throw new InvalidOperationException($"Unknown migration point {pointToMigrate}");
            }

            SetMigrationPoint(resourceId.ConnectionString, pointToMigrate);
        }
    }
}