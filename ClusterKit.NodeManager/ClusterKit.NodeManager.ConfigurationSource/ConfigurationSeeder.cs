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
    
    using ClusterKit.Data;
    using ClusterKit.NodeManager.Client.ApiSurrogates;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Client;

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
            if (context.Templates.Any())
            {
                return;
            }

            SetupUsers(context);

            context.Templates.AddRange(this.GetDefaultTemplates());
            var nugetFeeds = new List<NugetFeed>();
            List<string> seedsFromConfig = new List<string>();
            List<PackageDescription> initialPackages = new List<PackageDescription>();

            try
            {
                var system = ServiceLocator.Current.GetInstance<ActorSystem>();
                var nugetUrl = system.Settings.Config.GetString("ClusterKit.NodeManager.PackageRepository");
                var nugetRepository = PackageRepositoryFactory.Default.CreateRepository(nugetUrl);

                if (nugetRepository != null)
                {
                    initialPackages =
                        nugetRepository.Search(string.Empty, true)
                            .Where(p => p.IsLatestVersion)
                            .ToList()
                            .Select(p => new PackageDescription { Name = p.Id, Version = p.Version.ToString() })
                            .ToList();
                }

                seedsFromConfig =
                    Cluster.Get(system)
                        .Settings.SeedNodes.Select(
                            address => $"{address.Protocol}://{address.System}@{address.Host}:{address.Port}")
                        .ToList();

                var config = system.Settings.Config.GetConfig("ClusterKit.NodeManager.DefaultNugetFeeds");

                if (config != null)
                {
                    int feedTemplate = 0;
                    foreach (var pair in config.AsEnumerable())
                    {
                        var feedConfig = config.GetConfig(pair.Key);

                        NugetFeed.EnFeedType feedType;
                        if (!Enum.TryParse(feedConfig.GetString("type"), out feedType))
                        {
                            feedType = NugetFeed.EnFeedType.Private;
                        }

                        nugetFeeds.Add(
                            new NugetFeed
                                {
                                    Address = feedConfig.GetString("address"),
                                    Type = feedType,
                                    Id = feedTemplate++
                                });
                    }
                }
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }

            var defaultTemplates =
                this.GetDefaultTemplates()
                    .Select(
                        t =>
                            new Template
                                {
                                    Name = t.Name,
                                    Code = t.Code,
                                    Configuration = t.Configuration,
                                    ContainerTypes = t.ContainerTypes,
                                    MinimumRequiredInstances = t.MinimumRequiredInstances,
                                    MaximumNeededInstances = t.MinimumRequiredInstances,
                                    Priority = t.Priority,
                                    PackageRequirements =
                                        t.Packages.Select(p => new Template.PackageRequirement(p, null))
                                            .ToList()
                                })
                    .ToList();

            var configuration = new ReleaseConfiguration
                                    {
                                        NodeTemplates = new List<Template>(defaultTemplates),
                                        Packages = initialPackages,
                                        SeedAddresses = seedsFromConfig,
                                        NugetFeeds = nugetFeeds
                                    };
            var initialRelease = new Release
                                     {
                                         State = Release.EnState.Active,
                                         Name = "Initial configuration",
                                         Started = DateTimeOffset.Now,
                                         Configuration = configuration
                                     };
            context.Releases.Add(initialRelease);

            context.SaveChanges();
        }

        /// <summary>
        /// Gets the default templates
        /// </summary>
        /// <returns>The list of default templates</returns>
        protected virtual IEnumerable<NodeTemplate> GetDefaultTemplates()
        {
            yield return new NodeTemplate
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
                                                "ClusterKit.Monitoring.Client",
                                            },
                                    Configuration = Configurations.Publisher,
                                    Version = 0
                                };
            yield return new NodeTemplate
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
                                              "ClusterKit.Data.EF.Npgsql",
                                              "ClusterKit.Web.Swagger.Monitor",
                                              "ClusterKit.Web.Swagger",
                                              "ClusterKit.Log.Console",
                                              "ClusterKit.Log.ElasticSearch",
                                              "ClusterKit.Web.Authentication",
                                              "ClusterKit.Security.SessionRedis",
                                              "ClusterKit.API.Endpoint",
                                              "ClusterKit.Web.GraphQL.Publisher"
                                          },
                                  Configuration = Configurations.ClusterManager,
                                  Version = 0
                              };

            yield return new NodeTemplate
                            {
                                Code = "empty",
                                Name = "Cluster empty instance, just for demo",
                                MinimumRequiredInstances = 0,
                                MaximumNeededInstances = null,
                                ContainerTypes = new List<string> { "worker" },
                                Priority = 1.0,
                                Packages =
                                    new List<string>
                                        {
                                            "ClusterKit.Core.Service",
                                            "ClusterKit.NodeManager.Client",
                                            "ClusterKit.Monitoring.Client"
                                        },
                                Configuration = Configurations.Empty,
                                Version = 0
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
                                                $"{Privileges.NodeTemplate}.GetList",
                                                $"{Privileges.NodeTemplate}.Query"
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