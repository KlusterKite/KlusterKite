// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing basic work with <see cref="Release" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;

    using Xunit;
    
    /// <summary>
    /// Testing basic work with <see cref="Release"/>
    /// </summary>
    public class ReleaseTests
    {
        /// <summary>
        /// Tests the release compatibility set-up - templates with changed modules versions are incompatible
        /// </summary>
        [Fact]
        public void TestReleaseCompatibilityModuleVersionSet()
        {
            using (var connection = Effort.DbConnectionFactory.CreatePersistent(Guid.NewGuid().ToString("N")))
            {
                this.CreateTestDatabase(connection);

                var template1 = new NodeTemplate
                                    {
                                        Code = "template1",
                                        Configuration = "1",
                                        PackageRequirements = this.GetPackageRequirements("p1; p2")
                                    };

                var template2 = new NodeTemplate
                                    {
                                        Code = "template2",
                                        Configuration = "2",
                                        PackageRequirements = this.GetPackageRequirements("p2; p3")
                                    };

                var release = new Release
                                  {
                                      MinorVersion = 1,
                                      Name = "1",
                                      State = EnReleaseState.Active,
                                      Configuration =
                                          new ReleaseConfiguration
                                              {
                                                  NodeTemplates =
                                                      this.GetList(template1, template2),
                                                  Packages =
                                                      this.CreatePackages(
                                                          "p1;0.0.1",
                                                          "p2;0.0.1",
                                                          "p3;0.0.2")
                                              }
                                  };

                using (var context = new ConfigurationContext(connection, false))
                {
                    var compatibleTemplates =
                        release.GetCompatibleTemplates(context)
                            .OrderByDescending(t => t.CompatibleReleaseId)
                            .ThenBy(o => o.TemplateCode)
                            .ToList();
                    Assert.Equal(2, compatibleTemplates.Count);
                    Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());
                    Assert.Equal(2, compatibleTemplates.Select(t => t.CompatibleReleaseId).First());
                    Assert.Equal(1, compatibleTemplates.Select(t => t.CompatibleReleaseId).Skip(1).First());
                }
            }
        }

        /// <summary>
        /// Tests the release compatibility set-up - templates with changed modules list are incompatible
        /// </summary>
        [Fact]
        public void TestReleaseCompatibilityModuleListSet()
        {
            using (var connection = Effort.DbConnectionFactory.CreatePersistent(Guid.NewGuid().ToString("N")))
            {
                this.CreateTestDatabase(connection);

                var template1 = new NodeTemplate
                                    {
                                        Code = "template1",
                                        Configuration = "1",
                                        PackageRequirements = this.GetPackageRequirements("p1; p2")
                                    };

                var template2 = new NodeTemplate
                                    {
                                        Code = "template2",
                                        Configuration = "2",
                                        PackageRequirements = this.GetPackageRequirements("p1; p2; p3")
                                    };

                var release = new Release
                                  {
                                      MinorVersion = 1,
                                      Name = "1",
                                      State = EnReleaseState.Active,
                                      Configuration =
                                          new ReleaseConfiguration
                                              {
                                                  NodeTemplates =
                                                      this.GetList(template1, template2),
                                                  Packages =
                                                      this.CreatePackages(
                                                          "p1;0.0.1",
                                                          "p2;0.0.1",
                                                          "p3;0.0.1")
                                              }
                                  };

                using (var context = new ConfigurationContext(connection, false))
                {
                    var compatibleTemplates =
                        release.GetCompatibleTemplates(context)
                            .OrderByDescending(t => t.CompatibleReleaseId)
                            .ThenBy(o => o.TemplateCode)
                            .ToList();
                    Assert.Equal(2, compatibleTemplates.Count);
                    Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());
                    Assert.Equal(2, compatibleTemplates.Select(t => t.CompatibleReleaseId).First());
                    Assert.Equal(1, compatibleTemplates.Select(t => t.CompatibleReleaseId).Skip(1).First());
                }
            }
        }

        /// <summary>
        /// Tests the release compatibility set-up - templates with changed configurations are in compatible
        /// </summary>
        [Fact]
        public void TestReleaseCompatibilityConfigurationsSet()
        {
            using (var connection = Effort.DbConnectionFactory.CreatePersistent(Guid.NewGuid().ToString("N")))
            {
                this.CreateTestDatabase(connection);

                var template1 = new NodeTemplate
                                    {
                                        Code = "template1",
                                        Configuration = "1",
                                        PackageRequirements = this.GetPackageRequirements("p1; p2")
                                    };

                var template2 = new NodeTemplate
                                    {
                                        Code = "template2",
                                        Configuration = "2",
                                        PackageRequirements = this.GetPackageRequirements("p2; p3")
                                    };

                var release = new Release
                                  {
                                      MinorVersion = 1,
                                      Name = "1",
                                      State = EnReleaseState.Active,
                                      Configuration =
                                          new ReleaseConfiguration
                                              {
                                                  NodeTemplates =
                                                      this.GetList(template1, template2),
                                                  Packages =
                                                      this.CreatePackages(
                                                          "p1;0.0.1",
                                                          "p2;0.0.1",
                                                          "p3;0.0.1")
                                              }
                                  };

                using (var context = new ConfigurationContext(connection, false))
                {
                    var compatibleTemplates =
                        release.GetCompatibleTemplates(context)
                            .OrderByDescending(t => t.CompatibleReleaseId)
                            .ThenBy(o => o.TemplateCode)
                            .ToList();
                    Assert.Equal(2, compatibleTemplates.Count);
                    Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());
                    Assert.Equal(2, compatibleTemplates.Select(t => t.CompatibleReleaseId).First());
                    Assert.Equal(1, compatibleTemplates.Select(t => t.CompatibleReleaseId).Skip(1).First());
                }
            }
        }

        /// <summary>
        /// Creates the database with test data
        /// </summary>
        /// <param name="connection">The database connection</param>
        private void CreateTestDatabase(DbConnection connection)
        {
            var template1 = new NodeTemplate
            {
                Code = "template1",
                Configuration = "1",
                PackageRequirements = this.GetPackageRequirements("p1; p2")
            };

            var template2 = new NodeTemplate
            {
                Code = "template2",
                Configuration = "1",
                PackageRequirements = this.GetPackageRequirements("p2; p3")
            };

            using (var context = new ConfigurationContext(connection, false))
            {
                var release1 = new Release
                                   {
                                       MinorVersion = 1,
                                       Name = "1",
                                       State = EnReleaseState.Obsolete,
                                       Configuration =
                                           new ReleaseConfiguration
                                               {
                                                   NodeTemplates =
                                                       this.GetList(template1, template2),
                                                   Packages =
                                                       this.CreatePackages(
                                                           "p1;0.0.1",
                                                           "p2;0.0.1",
                                                           "p3;0.0.1")
                                               }
                                   };
                context.Releases.Add(release1);
                context.SaveChanges();
            }

            using (var context = new ConfigurationContext(connection, false))
            {
                var activeRelease = new Release
                                        {
                                            MinorVersion = 2,
                                            Name = "active",
                                            State = EnReleaseState.Active,
                                            Configuration =
                                                new ReleaseConfiguration
                                                    {
                                                        NodeTemplates =
                                                            this.GetList(
                                                                template1,
                                                                template2),
                                                        Packages =
                                                            this.CreatePackages(
                                                                "p1;0.0.1",
                                                                "p2;0.0.1",
                                                                "p3;0.0.1")
                                                    }
                                        };

                context.Releases.Add(activeRelease);
                activeRelease.CompatibleTemplates = new List<CompatibleTemplate>();
                activeRelease.CompatibleTemplates.Add(
                    new CompatibleTemplate { CompatibleReleaseId = 1, TemplateCode = template1.Code });
                activeRelease.CompatibleTemplates.Add(
                    new CompatibleTemplate { CompatibleReleaseId = 1, TemplateCode = template2.Code });
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates a list of objects from an array
        /// </summary>
        /// <typeparam name="T">The type of objects</typeparam>
        /// <param name="objects">The array of objects</param>
        /// <returns>The list of objects</returns>
        private List<T> GetList<T>(params T[] objects)
        {
            return objects.ToList();
        }

        /// <summary>
        /// The create a package list from string descriptions.
        /// </summary>
        /// <param name="packages">
        /// The list of package string descriptions.
        /// </param>
        /// <returns>
        /// The list of packages
        /// </returns>
        private List<PackageDescription> CreatePackages(params string[] packages)
        {
            return packages.Select(
                v =>
                    {
                        var split = v.Split(new[] { ";" }, StringSplitOptions.None);
                        return new PackageDescription(split[0], split[1]);
                    }).ToList();
        }

        /// <summary>
        /// Creates the list of package requirements from string description
        /// </summary>
        /// <param name="description">The requirements description</param>
        /// <returns>The list of requirements</returns>
        private List<NodeTemplate.PackageRequirement> GetPackageRequirements(string description)
        {
            var packages = description.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries);
            return packages.Select(
                p =>
                    {
                        var parts = p.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        return new NodeTemplate.PackageRequirement(parts[0], parts.Skip(1).FirstOrDefault());
                    }).ToList();
        }
    }
}
