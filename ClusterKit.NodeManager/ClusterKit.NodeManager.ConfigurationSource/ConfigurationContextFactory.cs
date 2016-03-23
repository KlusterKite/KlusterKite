using System;
using System.Linq;
using System.Threading.Tasks;

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using Akka.Actor;
    using Akka.Cluster;

    using ClusterKit.Core.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// The <seealso cref="ConfigurationContext"/> creation factory
    /// </summary>
    [UsedImplicitly]
    public class ConfigurationContextFactory : BaseContextFactory<ConfigurationContext, Migrations.Configuration>
    {
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
            context.InitEmptyTemplates();

            if (!context.SeedAddresses.Any())
            {
                var seedsFromConfig =
                    Cluster.Get(this.akkaSystem)
                        .Settings.SeedNodes.Select(
                            address =>
                            new SeedAddress
                            {
                                Address =
                                        $"{address.Protocol}://{address.System}@{address.Host}:{address.Port}"
                            });

                foreach (var seedAddress in seedsFromConfig)
                {
                    context.SeedAddresses.Add(seedAddress);
                }

                context.SaveChanges();
            }

            if (!context.NugetFeeds.Any())
            {
                var config = this.akkaSystem.Settings.Config.GetConfig("ClusterKit.NodeManager.DefaultNugetFeeds");
                if (config != null)
                {
                    foreach (var pair in config.AsEnumerable())
                    {
                        var feedConfig = config.GetConfig(pair.Key);

                        NugetFeed.EnFeedType feedType;
                        if (!Enum.TryParse(feedConfig.GetString("type"), out feedType))
                        {
                            feedType = NugetFeed.EnFeedType.Private;
                        }

                        context.NugetFeeds.Add(
                            new NugetFeed { Address = feedConfig.GetString("address"), Type = feedType });
                    }

                    context.SaveChanges();
                }
            }

            return context;
        }
    }
}