// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Seeder.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Seeds the <see cref="ConfigurationContext" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource.Seeder
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using Akka.Configuration;

    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource.Migrator.Migrations;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Migrator;
    using ClusterKit.Security.Attributes;

    using JetBrains.Annotations;

    using NuGet;

    /// <summary>
    /// Seeds the <see cref="ConfigurationContext"/>
    /// </summary>
    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class Seeder : BaseSeeder
    {
        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        [UsedImplicitly]
        protected const string ConfigConnectionStringPath = "ClusterKit.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// Akka configuration path to connection string
        /// </summary>
        [UsedImplicitly]
        protected const string ConfigDatabaseNamePath = "ClusterKit.NodeManager.ConfigurationDatabaseName";

        /// <summary>
        /// The seeder config
        /// </summary>
        private readonly Config config;

        /// <summary>
        /// The connection manager
        /// </summary>
        private readonly BaseConnectionManager connectionManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seeder"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public Seeder(Config config, BaseConnectionManager connectionManager)
        {
            this.config = config;
            this.connectionManager = connectionManager;
        }

        /// <inheritdoc />
        public override void Seed()
        {
            var connectionString = this.config.GetString(ConfigConnectionStringPath);
            var databaseName = this.connectionManager.EscapeDatabaseName(this.config.GetString(ConfigDatabaseNamePath));
            using (var connection = this.connectionManager.CreateConnection(connectionString))
            {
                connection.Open();
                Console.WriteLine(@"Opened the connection");

                if (this.connectionManager.CheckDatabaseExistence(connection, databaseName))
                {
                    Console.WriteLine(@"ClusterKit configuration database is already existing");
                    return;
                }

                this.connectionManager.CheckCreateDatabase(connection, databaseName);
                this.connectionManager.SwitchDatabase(connection, databaseName);

                using (var context = new ConfigurationContext(connection, false))
                {
                    var migrator = new MigrateDatabaseToLatestVersion<ConfigurationContext, Configuration>(true);
                    migrator.InitializeDatabase(context);

                    this.SetupUsers(context);
                    var repository = PackageRepositoryFactory.Default.CreateRepository(this.config.GetString("Nuget"));
                    var configuration = new ReleaseConfiguration
                                            {
                                                NodeTemplates = this.GetNodeTemplates().ToList(),
                                                MigratorTemplates = this.GetMigratorTemplates().ToList(),
                                                Packages = this.GetPackageDescriptions(repository).ToList(),
                                                SeedAddresses = this.GetSeeds().ToList(),
                                                NugetFeeds = this.GetNugetFeeds().ToList()
                                            };

                    var initialRelease = new Release
                                             {
                                                 State = EnReleaseState.Active,
                                                 Name = "Initial configuration",
                                                 Started = DateTimeOffset.Now,
                                                 Configuration = configuration
                                             };

                    var supportedFrameworks = this.config.GetStringList("ClusterKit.NodeManager.SupportedFrameworks");
                    var initialErrors = initialRelease.SetPackagesDescriptionsForTemplates(
                        repository,
                        supportedFrameworks.ToList());

                    foreach (var errorDescription in initialErrors)
                    {
                        Console.WriteLine($@"error in {errorDescription.Field} - {errorDescription.Message}");
                    }

                    context.Releases.Add(initialRelease);
                    context.SaveChanges();
                }
            }

            Console.WriteLine(@"ClusterKit configuration database created");
        }

        /// <summary>
        /// Gets the list of akka cluster seeds
        /// </summary>
        /// <returns>The list of seed addresses</returns>
        [UsedImplicitly]
        protected virtual IEnumerable<string> GetSeeds()
        {
            return this.config.GetStringList("ClusterKit.NodeManager.Seeds");
        }

        /// <summary>
        /// Get the list of node templates
        /// </summary>
        /// <returns>The list of node templates</returns>
        [UsedImplicitly]
        protected virtual IEnumerable<NodeTemplate> GetNodeTemplates()
        {
            yield return new NodeTemplate
                             {
                                 Code = "publisher",
                                 Name = "Cluster Nginx configurator",
                                 MinimumRequiredInstances = 1,
                                 MaximumNeededInstances = null,
                                 ContainerTypes = new List<string> { "publisher" },
                                 Priority = 1000.0,
                                 PackageRequirements =
                                     new[]
                                         {
                                             "ClusterKit.Core.Service",
                                             "ClusterKit.Web.NginxConfigurator",
                                             "ClusterKit.NodeManager.Client",
                                             "ClusterKit.Log.Console",
                                             "ClusterKit.Log.ElasticSearch",
                                             "ClusterKit.Monitoring.Client",
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = Configurations.Publisher
                             };
            yield return new NodeTemplate
                             {
                                 Code = "clusterManager",
                                 Name = "Cluster manager (cluster monitoring and managing)",
                                 MinimumRequiredInstances = 1,
                                 MaximumNeededInstances = 3,
                                 ContainerTypes = new List<string> { "manager", "worker" },
                                 Priority = 100.0,
                                 PackageRequirements =
                                     new[]
                                         {
                                             "ClusterKit.Core.Service",
                                             "ClusterKit.NodeManager.Client",
                                             "ClusterKit.Monitoring.Client",
                                             "ClusterKit.Monitoring",
                                             "ClusterKit.NodeManager",
                                             "ClusterKit.Data.EF.Npgsql",
                                             "ClusterKit.Web.Swagger.Monitor",
                                             "ClusterKit.Web.Swagger",
                                             "ClusterKit.Log.Console",
                                             "ClusterKit.Log.ElasticSearch",
                                             "ClusterKit.Web.Authentication",
                                             "ClusterKit.Security.SessionRedis",
                                             "ClusterKit.API.Endpoint",
                                             "ClusterKit.Web.GraphQL.Publisher"
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = Configurations.ClusterManager,
                             };

            yield return new NodeTemplate
                             {
                                 Code = "empty",
                                 Name = "Cluster empty instance, just for demo",
                                 MinimumRequiredInstances = 0,
                                 MaximumNeededInstances = null,
                                 ContainerTypes = new List<string> { "worker" },
                                 Priority = 1.0,
                                 PackageRequirements =
                                     new[]
                                         {
                                             "ClusterKit.Core.Service",
                                             "ClusterKit.NodeManager.Client",
                                             "ClusterKit.Monitoring.Client"
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = Configurations.Empty,
                             };
        }

        /// <summary>
        /// Get the list of migrator templates
        /// </summary>
        /// <returns>The list of migrator templates</returns>
        protected virtual IEnumerable<MigratorTemplate> GetMigratorTemplates()
        {
            yield return new MigratorTemplate
                             {
                                 Name = "ClusterKit Migrator",
                                 Code = "ClusterKit",
                                 Configuration = Configurations.Migrator,
                                 PackageRequirements =
                                     new[]
                                         {
                                             new NodeTemplate.PackageRequirement(
                                                 "ClusterKit.NodeManager.ConfigurationSource.Migrator",
                                                 null),
                                            new NodeTemplate.PackageRequirement(
                                                 "ClusterKit.NodeManager",
                                                 null),
                                             new NodeTemplate.PackageRequirement(
                                                 "ClusterKit.Data.EF.Npgsql",
                                                 null),
                                         }.ToList(),
                                 Priority = 1d
                             };
        }

        /// <summary>
        /// Get the list of package descriptions
        /// </summary>
        /// <param name="repository">The package repository</param>
        /// <returns>The list of package descriptions</returns>
        protected virtual IEnumerable<PackageDescription> GetPackageDescriptions(IPackageRepository repository)
        {
            return repository.Search(string.Empty, true)
                .Where(p => p.IsLatestVersion)
                .ToList()
                .Select(p => new PackageDescription(p.Id, p.Version.ToString()));
        }

        /// <summary>
        /// Gets the list of nuget feeds
        /// </summary>
        /// <returns>The list of nuget feeds</returns>
        protected virtual IEnumerable<NugetFeed> GetNugetFeeds()
        {
            var nugetFeedsConfig = this.config.GetConfig("ClusterKit.NodeManager.NugetFeeds");

            if (nugetFeedsConfig != null)
            {
                foreach (var pair in nugetFeedsConfig.AsEnumerable())
                {
                    var feedConfig = nugetFeedsConfig.GetConfig(pair.Key);

                    NugetFeed.EnFeedType feedType;
                    if (!Enum.TryParse(feedConfig.GetString("type"), out feedType))
                    {
                        feedType = NugetFeed.EnFeedType.Private;
                    }

                   yield return new NugetFeed { Address = feedConfig.GetString("address"), Type = feedType };
                }
            }
        }

        /// <summary>
        /// Installs default users and roles to the empty database
        /// </summary>
        /// <param name="context">The data context</param>
        [UsedImplicitly]
        protected virtual void SetupUsers(ConfigurationContext context)
        {
            if (context.Users.Any() || context.Roles.Any())
            {
                return;
            }

            var adminPrivileges = new List<IEnumerable<PrivilegeDescription>>
                                      {
                                          Utils.GetDefinedPrivileges(typeof(Privileges)),
                                          Utils.GetDefinedPrivileges(typeof(Web.Swagger.Messages.Privileges)),
                                          Utils.GetDefinedPrivileges(typeof(Monitoring.Client.Privileges))
                                      };

            var adminRole = new Role
                                {
                                    Uid = Guid.NewGuid(),
                                    Name = "Admin",
                                    AllowedScope =
                                        adminPrivileges.SelectMany(l => l.Select(p => p.Privilege)).ToList()
                                };
            var guestRole = new Role
                                {
                                    Uid = Guid.NewGuid(),
                                    Name = "Guest",
                                    AllowedScope =
                                        new List<string>
                                            {
                                                Privileges.GetActiveNodeDescriptions,
                                                Privileges.GetTemplateStatistics,
                                                $"{Privileges.Release}.Query"
                                            }
                                };
            context.Roles.Add(adminRole);
            context.Roles.Add(guestRole);

            var adminUser = new User { Uid = Guid.NewGuid(), Login = "admin", Roles = new List<Role> { adminRole } };
            adminUser.SetPassword("admin");
            var guestUser = new User { Uid = Guid.NewGuid(), Login = "guest", Roles = new List<Role> { guestRole } };
            guestUser.SetPassword("guest");

            context.Users.Add(adminUser);
            context.Users.Add(guestUser);
        }
    }
}
