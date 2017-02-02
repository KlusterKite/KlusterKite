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

    using ClusterKit.Data;
    using ClusterKit.NodeManager.Client;
    using ClusterKit.NodeManager.Client.ORM;

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
            SetupTemplates(context);

            context.SaveChanges();
        }

        /// <summary>
        /// Installs default templates to the empty database
        /// </summary>
        /// <param name="context">The data context</param>
        private static void SetupTemplates(ConfigurationContext context)
        {
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
                                    "ClusterKit.Monitoring.Client",
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
                                    "ClusterKit.Data.EF.Npgsql",
                                    "ClusterKit.Web.Swagger.Monitor",
                                    "ClusterKit.Web.Swagger",
                                    "ClusterKit.Log.Console",
                                    "ClusterKit.Log.ElasticSearch",
                                    "ClusterKit.Web.Authentication",
                                    "ClusterKit.Security.SessionRedis",
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
                            new List<string>
                                {
                                    "ClusterKit.Core.Service",
                                    "ClusterKit.NodeManager.Client",
                                    "ClusterKit.Monitoring.Client"
                                },
                        Configuration = Configurations.Empty,
                        Version = 0
                    });
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

            var adminRole = new Role
                                {
                                    Uid = Guid.NewGuid(),
                                    Name = "Admin",
                                    AllowedScope = new List<string>(Security.Client.Utils.DefinedPrivileges.Select(d => d.Privilege))
                                };
            var guestRole = new Role
                                {
                                    Uid = Guid.NewGuid(),
                                    Name = "Guest",
                                    AllowedScope = new List<string> { Privileges.GetNodeList }
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