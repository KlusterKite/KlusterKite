// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The <seealso cref="ConfigurationContext" /> creation factory
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.Data;
    using ClusterKit.Data.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// The <seealso cref="ConfigurationContext"/> creation factory
    /// </summary>
    [UsedImplicitly]
    public class ConfigurationContextFactory : BaseContextFactory<ConfigurationContext, Migrations.Configuration>
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem akkaSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationContextFactory"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        /// <param name="akkaSystem">Akka system</param>
        public ConfigurationContextFactory(BaseConnectionManager connectionManager, ActorSystem akkaSystem)
            : base(connectionManager)
        {
            this.akkaSystem = akkaSystem;
        }

        /// <summary>
        /// Creates context attached to datasource.
        ///             Datasource will be modified (database will be created, migrations will be run).
        /// </summary>
        /// <param name="connectionString">The connection String.
        ///             </param><param name="databaseName">The database Name.
        ///             </param>
        /// <returns>
        /// The data context
        /// </returns>
        public override async Task<ConfigurationContext> CreateAndUpgradeContext(string connectionString, string databaseName)
        {
            var context = await base.CreateAndUpgradeContext(connectionString, databaseName);
            var seederTypeName = this.akkaSystem.Settings.Config.GetString("ClusterKit.NodeManager.ConfigurationSeederType");
            this.akkaSystem.Log.Warning("{Type}: Using seeder {SeederTypeName}", this.GetType().Name, seederTypeName ?? "default");

            var seederType = string.IsNullOrWhiteSpace(seederTypeName) ? null : Type.GetType(seederTypeName);

            var seeder = seederType == null
                             ? new ConfigurationSeeder()
                             : (IDataSeeder<ConfigurationContext>)Activator.CreateInstance(seederType);
            seeder.Seed(context);
            return context;
        }
    }
}