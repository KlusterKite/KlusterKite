// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMigrator.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the TestMigrator type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests.Migrations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Akka.Configuration;

    using JetBrains.Annotations;

    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The test migrator
    /// </summary>
    [UsedImplicitly]
    public abstract class TestMigrator : IMigrator
    {
        /// <inheritdoc />
        public TestMigrator(Config config)
        {
            this.Config = config;
        }

        /// <inheritdoc />
        public abstract EnResourceDependencyType DependencyType { get; }

        /// <inheritdoc />
        public string LatestPoint => this.DefinedMigrationPoints?.LastOrDefault();

        /// <inheritdoc />
        public abstract string Name { get; }

        /// <inheritdoc />
        public decimal Priority => 1M;

        /// <summary>
        /// Gets the migrator config
        /// </summary>
        protected Config Config { get; }

        /// <summary>
        /// Gets the list of defined migration points
        /// </summary>
        protected abstract List<string> DefinedMigrationPoints { get; }

        /// <summary>
        /// Gets the list of defined migration resources
        /// </summary>
        protected abstract List<string> Resources { get; }

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
            if (this.Config.GetBoolean("TestMigrator.ThrowOnGetAllPoints"))
            {
                throw new Exception("GetAllPoints failed");
            }

            return this.DefinedMigrationPoints;
        }

        /// <inheritdoc />
        public string GetCurrentPoint(ResourceId resourceId)
        {
            if (this.Config.GetBoolean("TestMigrator.ThrowOnGetCurrentPoint"))
            {
                throw new Exception("GetCurrentPoint failed");
            }

            if (!this.Resources.Contains(resourceId.ConnectionString))
            {
                throw new InvalidOperationException($"Unknown connection {resourceId.ConnectionString}");
            }

            var path = Path.Combine(this.Config.GetString("TestMigrator.Dir"), resourceId.ConnectionString);

            return File.Exists(path)
                       ? File.ReadAllText(path)
                       : null;
        }

        /// <inheritdoc />
        public IEnumerable<ResourceId> GetMigratableResources()
        {
            if (this.Config.GetBoolean("TestMigrator.ThrowOnGetMigratableResources"))
            {
                throw new Exception("GetMigratableResources failed");
            }

            foreach (var resource in this.Resources)
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
        public IEnumerable<string> Migrate(ResourceId resourceId, string pointToMigrate)
        {
            if (this.Config.GetBoolean("TestMigrator.ThrowOnMigrate"))
            {
                throw new Exception("Migrate failed");
            }

            if (!this.Resources.Contains(resourceId.ConnectionString))
            {
                throw new InvalidOperationException($"Unknown connection {resourceId.ConnectionString}");
            }

            if (!this.DefinedMigrationPoints.Contains(pointToMigrate))
            {
                throw new InvalidOperationException($"Unknown migration point {pointToMigrate} among {string.Join(", ", this.DefinedMigrationPoints)}");
            }

            var path = Path.Combine(this.Config.GetString("TestMigrator.Dir"), resourceId.ConnectionString);
            SetMigrationPoint(path, pointToMigrate);
            yield return "success";
        }

        /// <summary>
        /// The dependence resource
        /// </summary>
        public class Dependence : TestMigrator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestMigrator.Dependence"/> class.
            /// </summary>
            /// <param name="config">
            /// The config.
            /// </param>
            public Dependence(Config config)
                : base(config)
            {
            }

            /// <inheritdoc />
            public override EnResourceDependencyType DependencyType => EnResourceDependencyType.CodeDependsOnResource;

            /// <inheritdoc />
            public override string Name => "Test dependence resource";

            /// <inheritdoc />
            protected override List<string> DefinedMigrationPoints => this.Config
                .GetStringList("TestMigrator.Dependence.DefinedMigrationPoints").ToList();

            /// <inheritdoc />
            protected override List<string> Resources => this.Config.GetStringList("TestMigrator.Dependence.Resources").ToList();
        }

        /// <summary>
        /// The dependent resource
        /// </summary>
        public class Dependent : TestMigrator
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestMigrator.Dependent"/> class.
            /// </summary>
            /// <param name="config">
            /// The config.
            /// </param>
            public Dependent(Config config)
                : base(config)
            {
            }

            /// <inheritdoc />
            public override EnResourceDependencyType DependencyType => EnResourceDependencyType.ResourceDependsOnCode;

            /// <inheritdoc />
            public override string Name => "Test dependent resource";

            /// <inheritdoc />
            protected override List<string> DefinedMigrationPoints => this.Config
                .GetStringList("TestMigrator.Dependent.DefinedMigrationPoints").ToList();

            /// <inheritdoc />
            protected override List<string> Resources => this.Config.GetStringList("TestMigrator.Dependent.Resources").ToList();
        }
    }
}