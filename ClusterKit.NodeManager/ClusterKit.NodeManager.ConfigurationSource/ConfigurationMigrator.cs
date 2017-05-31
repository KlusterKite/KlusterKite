// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationMigrator.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The migrator for configuration database
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Collections.Generic;

    using Akka.Configuration;

    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Migrator;
    using ClusterKit.NodeManager.Migrator.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// The migrator for configuration database
    /// </summary>
    [UsedImplicitly]
    public class ConfigurationMigrator : BaseDatabaseMigrator<ConfigurationContext>
    {
        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        private const string ConfigConnectionStringPath = "ClusterKit.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        private const string ConfigDatabaseProviderNamePath = "ClusterKit.NodeManager.ConfigurationDatabaseProviderName";

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
        /// <param name="contextFactory">
        /// The context factory
        /// </param>
        public ConfigurationMigrator(Config config, UniversalContextFactory contextFactory) : base(contextFactory)
        {
            this.config = config;
        }

        /// <inheritdoc />
        public override string Name => "ClusterKit configuration database";

        /// <inheritdoc />
        public override IEnumerable<ResourceId> GetMigratableResources()
        {
            yield return this.GetDefaultResourceId();
        }

        /// <inheritdoc />
        public override ResourceId GetDefaultResourceId()
        {
            var connectionString = this.config.GetString(ConfigConnectionStringPath);
            var providerName = this.config.GetString(ConfigDatabaseProviderNamePath);
            return new ResourceId
                       {
                           Name = "ClusterKit configuration database",
                           ConnectionString = connectionString,
                           ProviderName = providerName,
                           Code = "configDB"
                       };
        }
    }
}
