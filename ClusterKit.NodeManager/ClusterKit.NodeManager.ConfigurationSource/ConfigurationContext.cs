// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationContext.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Configuration database context
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Linq;

    using JetBrains.Annotations;

    /// <summary>
    /// Configuration database context
    /// </summary>
    public class ConfigurationContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        /// <param name="existingConnection">
        /// The existing connection.
        /// </param>
        /// <param name="contextOwnsConnection">
        /// The context owns connection.
        /// </param>
        public ConfigurationContext(DbConnection existingConnection, bool contextOwnsConnection = true)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContext"/> class.
        /// </summary>
        public ConfigurationContext()
        {
        }

        /// <summary>
        /// Gets value indicating whether current context is test mock
        /// </summary>
        [UsedImplicitly]
        public virtual bool IsMoq { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of used NuGet feeds
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<NugetFeed> NugetFeeds { get; set; }

        /// <summary>
        ///  Gets or sets the list of addresses of cluster seeds
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<SeedAddress> SeedAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of node templates in database
        /// </summary>
        [UsedImplicitly]
        public virtual DbSet<NodeTemplate> Templates { get; set; }

        /// <summary>
        /// Initializes empty <seealso cref="Templates"/> with default data
        /// </summary>
        public virtual void InitEmptyTemplates()
        {
            if (this.Templates.Any())
            {
                return;
            }

            this.Templates.Add(
                new NodeTemplate
                {
                    Code = "seed",
                    Name = "Cluster seed and Nginx configurator",
                    MininmumRequiredInstances = 2,
                    MaximumNeededInstances = null,
                    ContainerTypes = new List<string> { "seed" },
                    Priority = 1000.0,
                    Packages =
                            new List<string>
                                {
                                    "ClusterKit.Core.Service",
                                    "ClusterKit.Web.NginxConfigurator",
                                    "ClusterKit.NodeManager.Client"
                                },
                    Configuration = Configurations.Seed,
                    Version = 0
                });

            this.Templates.Add(
                new NodeTemplate
                {
                    Code = "clusterManager",
                    Name = "Cluster manager (cluster monitoring and managing)",
                    MininmumRequiredInstances = 2,
                    MaximumNeededInstances = 3,
                    ContainerTypes = new List<string> { "manager", "worker" },
                    Priority = 100.0,
                    Packages =
                            new List<string>
                                {
                                    "ClusterKit.Core.Service",
                                    "ClusterKit.NodeManager.Client",
                                    "ClusterKit.Monitoring",
                                    "ClusterKit.NodeManager",
                                    "ClusterKit.Core.EF.Npgsql",
                                    "ClusterKit.Web.Swagger.Monitor",
                                    "ClusterKit.Web.Swagger"
                                },
                    Configuration = Configurations.ClusterManager,
                    Version = 0
                });

            this.Templates.Add(
                new NodeTemplate
                {
                    Code = "empty",
                    Name = "Cluster empty instance, just for demo",
                    MininmumRequiredInstances = 0,
                    MaximumNeededInstances = null,
                    ContainerTypes = new List<string> { "worker" },
                    Priority = 1.0,
                    Packages =
                            new List<string> { "ClusterKit.Core.Service", "ClusterKit.NodeManager.Client" },
                    Configuration = Configurations.Empty,
                    Version = 0
                });

            this.SaveChanges();
        }
    }
}