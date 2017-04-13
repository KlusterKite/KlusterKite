// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationMigrator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The migrator for configuration database
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Migrator
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using ClusterKit.NodeManager.ConfigurationSource.Migrator.Migrations;
    using ClusterKit.NodeManager.Migrator;
    using ClusterKit.NodeManager.Migrator.EF;
    
    /// <summary>
    /// The migrator for configuration database
    /// </summary>
    public class ConfigurationMigrator : BaseDatabaseMigrator<ConfigurationContext, Configuration>
    {
        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        private const string ConfigConnectionStringPath = "ClusterKit.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// The migrator config.
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationMigrator"/> class.
        /// </summary>
        /// <param name="config">
        /// The migrator config.
        /// </param>
        public ConfigurationMigrator(Config config)
        {
            this.config = config;
        }

        /// <inheritdoc />
        public override IEnumerable<ResourceId> GetMigratableResources()
        {
            var connectionString = this.config.GetString(ConfigConnectionStringPath);
            yield return new ResourceId
                             {
                                 Name = "ClusterKit configuration database",
                                 ConnectionString = connectionString
                             };
        }
    }
}
