﻿// --------------------------------------------------------------------------------------------------------------------
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
        /// Gets or sets the list of node templates in database
        /// </summary>
        [UsedImplicitly]
        public DbSet<NodeTemplate> Templates { get; set; }

        /// <summary>
        /// Initializes empty <seealso cref="Templates"/> with default data
        /// </summary>
        public void InitEmptyTemplates()
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
                    MaximumNeededInstances = 4,
                    ContainerTypes = new List<string> { "seed" },
                    Priority = 1000.0,
                    Packages = new List<string> { "ClusterKit.Core.Service", "ClusterKit.Web.NginxConfigurator, ClusterKit.NodeManager.Client" },
                    Configuration = Configurations.Seed
                });

            this.Templates.Add(
                new NodeTemplate
                {
                    Code = "clusterManager",
                    Name = "Cluster manager (cluster monitoring and managing)",
                    MininmumRequiredInstances = 2,
                    MaximumNeededInstances = 3,
                    ContainerTypes = new List<string> { "manager" },
                    Priority = 100.0,
                    Packages = new List<string> { "ClusterKit.Core.Service", "ClusterKit.Monitoring", "ClusterKit.NodeManager", "ClusterKit.Web.Swagger.Monitor" },
                    Configuration = Configurations.ClusterManager
                });

            this.Templates.Add(
               new NodeTemplate
               {
                   Code = "empty",
                   Name = "Cluster manager empty instance",
                   MininmumRequiredInstances = 0,
                   MaximumNeededInstances = null,
                   ContainerTypes = new List<string> { "worker" },
                   Priority = 1.0,
                   Packages = new List<string> { "ClusterKit.Core.Service", "ClusterKit.NodeManager.Client" },
                   Configuration = Configurations.Empty
               });

            this.SaveChanges();
        }
    }
}