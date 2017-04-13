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
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Akka.Actor;
    using Akka.Cluster;

    using Castle.Components.DictionaryAdapter;

    using ClusterKit.Data;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.Security.Attributes;

    using Microsoft.Practices.ServiceLocation;

    using NuGet;

    using Privileges = ClusterKit.NodeManager.Client.Privileges;

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
            if (context.Releases.Any())
            {
                return;
            }

            SetupUsers(context);

            var nugetFeeds = new List<NugetFeed>();
            List<string> seedsFromConfig = new List<string>();
            List<PackageDescription> initialPackages = new List<PackageDescription>();
            var system = ServiceLocator.Current.GetInstance<ActorSystem>();
            var nugetRepository = ServiceLocator.Current.GetInstance<IPackageRepository>();

            try
            {
                if (nugetRepository != null)
                {
                    initialPackages = nugetRepository.Search(string.Empty, true)
                        .Where(p => p.IsLatestVersion)
                        .ToList()
                        .Select(p => new PackageDescription { Id = p.Id, Version = p.Version.ToString() })
                        .ToList();
                }

                seedsFromConfig = Cluster.Get(system)
                    .Settings.SeedNodes.Select(
                        address => $"{address.Protocol}://{address.System}@{address.Host}:{address.Port}")
                    .ToList();

                var config = system.Settings.Config.GetConfig("ClusterKit.NodeManager.DefaultNugetFeeds");

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

                        nugetFeeds.Add(new NugetFeed { Address = feedConfig.GetString("address"), Type = feedType });
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            var defaultTemplates = this.GetDefaultTemplates().ToList();

            var migrators = new[]
                                {
                                    new MigratorTemplate
                                        {
                                            Name = "ClusterKit Migrator",
                                            Code = "ClusterKit",
                                            Configuration = Configurations.Migrator,
                                            PackageRequirements =
                                                new[]
                                                    {
                                                        new Template.PackageRequirement(
                                                            "ClusterKit.NodeManager.ConfigurationSource.Migrator",
                                                            null)
                                                    }.ToList(),
                                            Priority = 1d
                                        }
                                }.ToList();

            var configuration = new ReleaseConfiguration
                                    {
                                        NodeTemplates = defaultTemplates,
                                        MigratorTemplates = migrators,
                                        Packages = initialPackages,
                                        SeedAddresses = seedsFromConfig,
                                        NugetFeeds = nugetFeeds
                                    };
            var initialRelease = new Release
                                     {
                                         State = EnReleaseState.Active,
                                         Name = "Initial configuration",
                                         Started = DateTimeOffset.Now,
                                         Configuration = configuration
                                     };

            var supportedFrameworks =
                system.Settings.Config.GetStringList("ClusterKit.NodeManager.SupportedFrameworks");
            var initialErrors =
                initialRelease.SetPackagesDescriptionsForTemplates(nugetRepository, supportedFrameworks.ToList());

            foreach (var errorDescription in initialErrors)
            {
                system.Log.Error(
                    "{Type}: initial error in {Field} - {Message}",
                    this.GetType().Name,
                    errorDescription.Field,
                    errorDescription.Message);
            }

            context.Releases.Add(initialRelease);

            context.SaveChanges();
        }

        /// <summary>
        /// Gets the default templates
        /// </summary>
        /// <returns>The list of default templates</returns>
        protected virtual IEnumerable<Template> GetDefaultTemplates()
        {
            yield return new Template
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
                                            }.Select(p => new Template.PackageRequirement(p, null)).ToList(),
                                    Configuration = Configurations.Publisher
                                };
            yield return new Template
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
                                          }.Select(p => new Template.PackageRequirement(p, null)).ToList(),
                                  Configuration = Configurations.ClusterManager,
            };

            yield return new Template
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
                                        }.Select(p => new Template.PackageRequirement(p, null)).ToList(),
                                Configuration = Configurations.Empty,
                            };
        }

        /// <summary>
        /// Installs default users and roles to the empty database
        /// </summary>
        /// <param name="context">The data context</param>
        private static void SetupUsers(ConfigurationContext context)
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