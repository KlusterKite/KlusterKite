// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSeeder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Initializes empty configuration database with start data
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.Core.Data;

    /// <summary>
    /// Initializes empty configuration database with start data
    /// </summary>
    public class ConfigurationSeeder : IDataSeeder<ConfigurationContext>
    {
        /// <summary>
        /// Checks database for emptiness and fills with data
        /// </summary>
        /// <param name="context">Current opened context</param>
        public void Seed(ConfigurationContext context)
        {
            if (context.Templates.Any())
            {
                return;
            }

            context.Templates.Add(
                new NodeTemplate
                {
                    Code = "publisher",
                    Name = "Cluster Nginx configurator",
                    MinimumRequiredInstances = 1,
                    MaximumNeededInstances = null,
                    ContainerTypes = new List<string> { "publisher" },
                    Priority = 1000.0,
                    Packages =
                            new List<string>
                                {
                                    "ClusterKit.Core.Service",
                                    "ClusterKit.Web.NginxConfigurator",
                                    "ClusterKit.NodeManager.Client",
                                    "ClusterKit.Log.Console",
                                    "ClusterKit.Log.ElasticSearch",
                                    "ClusterKit.Monitoring.Client"
                                },
                    Configuration = Configurations.Publisher,
                    Version = 0
                });

            context.Templates.Add(
                new NodeTemplate
                {
                    Code = "clusterManager",
                    Name = "Cluster manager (cluster monitoring and managing)",
                    MinimumRequiredInstances = 1,
                    MaximumNeededInstances = 3,
                    ContainerTypes = new List<string> { "manager", "worker" },
                    Priority = 100.0,
                    Packages =
                            new List<string>
                                {
                                    "ClusterKit.Core.Service",
                                    "ClusterKit.NodeManager.Client",
                                    "ClusterKit.Monitoring.Client",
                                    "ClusterKit.Monitoring",
                                    "ClusterKit.NodeManager",
                                    "ClusterKit.Core.EF.Npgsql",
                                    "ClusterKit.Web.Swagger.Monitor",
                                    "ClusterKit.Web.Swagger",
                                    "ClusterKit.Log.Console",
                                    "ClusterKit.Log.ElasticSearch"
                                },
                    Configuration = Configurations.ClusterManager,
                    Version = 0
                });

            context.Templates.Add(
                new NodeTemplate
                {
                    Code = "empty",
                    Name = "Cluster empty instance, just for demo",
                    MinimumRequiredInstances = 0,
                    MaximumNeededInstances = null,
                    ContainerTypes = new List<string> { "worker" },
                    Priority = 1.0,
                    Packages =
                            new List<string> { "ClusterKit.Core.Service", "ClusterKit.NodeManager.Client", "ClusterKit.Monitoring.Client" },
                    Configuration = Configurations.Empty,
                    Version = 0
                });

            context.SaveChanges();
        }
    }
}