// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Seeder.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Seeds the <see cref="ConfigurationContext" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource.Seeder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;

    using Akka.Configuration;

    using JetBrains.Annotations;

    using KlusterKite.Data.EF;
    using KlusterKite.NodeManager.Client;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Migrator;
    using KlusterKite.Security.Attributes;

    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;

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
        protected const string ConfigConnectionStringPath = "KlusterKite.NodeManager.ConfigurationDatabaseConnectionString";

        /// <summary>
        /// Akka configuration path to database name
        /// </summary>
        [UsedImplicitly]
        protected const string ConfigDatabaseNamePath = "KlusterKite.NodeManager.ConfigurationDatabaseName";

        /// <summary>
        /// Akka configuration path to database provider name
        /// </summary>
        [UsedImplicitly]
        protected const string ConfigDatabaseProviderNamePath = "KlusterKite.NodeManager.ConfigurationDatabaseProviderName";

        /// <summary>
        /// The context factory
        /// </summary>
        private readonly UniversalContextFactory contextFactory;

        /// <summary>
        /// The package repository
        /// </summary>
        private readonly IPackageRepository packageRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="Seeder"/> class.
        /// </summary>
        /// <param name="config">
        /// The config.
        /// </param>
        /// <param name="contextFactory">
        /// The context factory.
        /// </param>
        /// <param name="packageRepository">
        /// The package repository.
        /// </param>
        public Seeder(Config config, UniversalContextFactory contextFactory, IPackageRepository packageRepository)
        {
            this.Config = config;
            this.contextFactory = contextFactory;
            this.packageRepository = packageRepository;
        }

        /// <summary>
        /// Gets the seeder configuration
        /// </summary>
        protected Config Config { get; }

        /// <inheritdoc />
        public override void Seed()
        {
            var connectionString = this.Config.GetString(ConfigConnectionStringPath);
            var databaseName = this.Config.GetString(ConfigDatabaseNamePath);
            var databaseProviderName = this.Config.GetString(ConfigDatabaseProviderNamePath);
            using (var context =
                this.contextFactory.CreateContext<ConfigurationContext>(
                    databaseProviderName,
                    connectionString,
                    databaseName))
            {
                if (databaseProviderName != "InMemory")
                {
                    var databaseCreator = context.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
                    if (databaseCreator == null)
                    {
                        Console.WriteLine(@"Error - could not check database existence. There is no IDatabaseCreator.");
                        return;
                    }

                    if (databaseCreator.Exists())
                    {
                        Console.WriteLine(@"KlusterKite configuration database is already existing");
                        return;
                    }

                    context.Database.Migrate();
                }

                this.SetupUsers(context);
                var configuration =
                    new ConfigurationSettings
                        {
                            NodeTemplates = this.GetNodeTemplates().ToList(),
                            MigratorTemplates = this.GetMigratorTemplates().ToList(),
                            Packages = this.GetPackageDescriptions().GetAwaiter().GetResult(),
                            SeedAddresses = this.GetSeeds().ToList(),
                            NugetFeed = this.Config.GetString("KlusterKite.NodeManager.PackageRepository")
                        };

                var initialConfiguration = new Configuration
                                         {
                                             State = EnConfigurationState.Active,
                                             Name = "Initial configuration",
                                             Started = DateTimeOffset.Now,
                                             Settings = configuration
                                         };

                var supportedFrameworks = this.Config.GetStringList("KlusterKite.NodeManager.SupportedFrameworks");
                var initialErrors =
                    initialConfiguration.SetPackagesDescriptionsForTemplates(this.packageRepository, supportedFrameworks.ToList()).GetAwaiter().GetResult();

                foreach (var errorDescription in initialErrors)
                {
                    Console.WriteLine($@"error in {errorDescription.Field} - {errorDescription.Message}");
                }

                context.Configurations.Add(initialConfiguration);
                context.SaveChanges();
            }

            Console.WriteLine(@"KlusterKite configuration database created");
        }

        /// <summary>
        /// Gets the list of akka cluster seeds
        /// </summary>
        /// <returns>The list of seed addresses</returns>
        [UsedImplicitly]
        protected virtual IEnumerable<string> GetSeeds()
        {
            return this.Config.GetStringList("KlusterKite.NodeManager.Seeds");
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
                                             "KlusterKite.Core.Service",
                                             "KlusterKite.Web.NginxConfigurator",
                                             "KlusterKite.NodeManager.Client",
                                             "KlusterKite.Log.Console",
                                             "KlusterKite.Log.ElasticSearch",
                                             "KlusterKite.Monitoring.Client",
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = ConfigurationUtils.ReadTextResource(this.GetType().GetTypeInfo().Assembly, "KlusterKite.NodeManager.ConfigurationSource.Seeder.Resources.publisher.hocon")
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
                                             "KlusterKite.Core.Service",
                                             "KlusterKite.NodeManager.Client",
                                             "KlusterKite.Monitoring.Client",
                                             "KlusterKite.Monitoring",
                                             "KlusterKite.NodeManager",
                                             "KlusterKite.Data.EF.Npgsql",
                                             "KlusterKite.Log.Console",
                                             "KlusterKite.Log.ElasticSearch",
                                             "KlusterKite.Web.Authentication",
                                             "KlusterKite.NodeManager.Authentication",
                                             "KlusterKite.Security.SessionRedis",
                                             "KlusterKite.API.Endpoint",
                                             "KlusterKite.Web.GraphQL.Publisher"
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = ConfigurationUtils.ReadTextResource(this.GetType().GetTypeInfo().Assembly, "KlusterKite.NodeManager.ConfigurationSource.Seeder.Resources.clusterManager.hocon")
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
                                             "KlusterKite.Core.Service",
                                             "KlusterKite.NodeManager.Client",
                                             "KlusterKite.Monitoring.Client"
                                         }.Select(p => new NodeTemplate.PackageRequirement(p, null)).ToList(),
                                 Configuration = ConfigurationUtils.ReadTextResource(this.GetType().GetTypeInfo().Assembly, "KlusterKite.NodeManager.ConfigurationSource.Seeder.Resources.empty.hocon")
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
                                 Name = "KlusterKite Migrator",
                                 Code = "KlusterKite",
                                 Configuration = ConfigurationUtils.ReadTextResource(this.GetType().GetTypeInfo().Assembly, "KlusterKite.NodeManager.ConfigurationSource.Seeder.Resources.migrator.hocon"),
                                 PackageRequirements =
                                     new[]
                                         {
                                            new NodeTemplate.PackageRequirement(
                                                 "KlusterKite.NodeManager",
                                                 null),
                                             new NodeTemplate.PackageRequirement(
                                                 "KlusterKite.NodeManager.Mock",
                                                 null),
                                             new NodeTemplate.PackageRequirement(
                                                 "KlusterKite.Data.EF.Npgsql",
                                                 null),
                                         }.ToList(),
                                 Priority = 1d
                             };
        }

        /// <summary>
        /// Get the list of package descriptions
        /// </summary>
        /// <returns>The list of package descriptions</returns>
        protected virtual async Task<List<PackageDescription>> GetPackageDescriptions()
        {
            return (await this.packageRepository.SearchAsync(string.Empty, true))
                .Select(p => new PackageDescription(p.Identity.Id, p.Identity.Version.ToString())).ToList();
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

            var adminPrivileges =
                new List<IEnumerable<PrivilegeDescription>>
                    {
                        Utils.GetDefinedPrivileges(typeof(Privileges)),

                        Utils.GetDefinedPrivileges(
                            typeof(Monitoring.Client.Privileges))
                    };

            var adminRole = new Role
                                {
                                    Uid = Guid.NewGuid(),
                                    Name = "Admin",
                                    AllowedScope = adminPrivileges.SelectMany(l => l.Select(p => p.Privilege))
                                        .ToList()
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
                                                $"{Privileges.Configuration}.Query"
                                            }
                                };
            context.Roles.Add(adminRole);
            context.Roles.Add(guestRole);

            var adminUser = new User
                                {
                                    Uid = Guid.NewGuid(),
                                    Login = "admin",
                                    Roles = new List<RoleUser> { new RoleUser { Role = adminRole } }
                                };
            adminUser.SetPassword("admin");
            var guestUser = new User
                                {
                                    Uid = Guid.NewGuid(),
                                    Login = "guest",
                                    Roles = new List<RoleUser> { new RoleUser { Role = guestRole } }
                                };
            guestUser.SetPassword("guest");

            context.Users.Add(adminUser);
            context.Users.Add(guestUser);
        }
    }
}
