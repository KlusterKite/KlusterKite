// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing basic work with <see cref="Configuration" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using KlusterKite.Data.EF.InMemory;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Messages;

    using Microsoft.EntityFrameworkCore;

    using Xunit;
    using Xunit.Abstractions;

    /// <summary>
    /// Testing basic work with <see cref="Configuration"/>
    /// </summary>
    [Collection("KlusterKite.NodeManager.Tests.ConfigurationContext")]
    public class ConfigurationTests : IDisposable
    {
        /// <summary>
        /// The output.
        /// </summary>
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationTests"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public ConfigurationTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.CreateContext().Database.EnsureDeleted();
        }

        /// <summary>
        /// Tests the configuration compatibility set-up - templates with changed configurations are in compatible
        /// </summary>
        [Fact]
        public void TestConfigurationCompatibilityConfigurationsSet()
        {
            this.CreateTestDatabase();

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

            var configuration = new Configuration
                              {
                                  MinorVersion = 1,
                                  Name = "1",
                                  State = EnConfigurationState.Active,
                                  Settings =
                                      new ConfigurationSettings
                                          {
                                              NodeTemplates =
                                                  this.GetList(template1, template2),
                                              Packages = this.CreatePackages(
                                                  "p1;0.0.1",
                                                  "p2;0.0.1",
                                                  "p3;0.0.1")
                                          }
                              };

            using (var context = this.CreateContext())
            {
                // facepalm - https://github.com/aspnet/EntityFramework/issues/6872
                var ids = context.Configurations.Select(r => r.Id).OrderByDescending(id => id).ToList();

                var compatibleTemplates = configuration.GetCompatibleTemplates(context)
                    .OrderByDescending(t => t.CompatibleConfigurationId).ThenBy(o => o.TemplateCode).ToList();
                Assert.Equal(2, compatibleTemplates.Count);
                Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());

                this.output.WriteLine($"Database configuration ids: {string.Join(", ", ids)}");
                this.output.WriteLine(
                    $"Compatible templates configuration ids: {string.Join(", ", compatibleTemplates.Select(t => t.CompatibleConfigurationId))}");

                Assert.Equal(ids[0], compatibleTemplates.Select(t => t.CompatibleConfigurationId).First());
                Assert.Equal(ids[1], compatibleTemplates.Select(t => t.CompatibleConfigurationId).Skip(1).First());
            }
        }

        /// <summary>
        /// Tests the configuration compatibility set-up - templates with changed modules list are incompatible
        /// </summary>
        [Fact]
        public void TestConfigurationCompatibilityModuleListSet()
        {
            this.CreateTestDatabase();

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

            var configuration = new Configuration
                              {
                                  MinorVersion = 1,
                                  Name = "1",
                                  State = EnConfigurationState.Active,
                                  Settings =
                                      new ConfigurationSettings
                                          {
                                              NodeTemplates =
                                                  this.GetList(template1, template2),
                                              Packages = this.CreatePackages(
                                                  "p1;0.0.1",
                                                  "p2;0.0.1",
                                                  "p3;0.0.1")
                                          }
                              };

            using (var context = this.CreateContext())
            {
                // facepalm - https://github.com/aspnet/EntityFramework/issues/6872
                var ids = context.Configurations.Select(r => r.Id).OrderByDescending(id => id).ToList();

                var compatibleTemplates = configuration.GetCompatibleTemplates(context)
                    .OrderByDescending(t => t.CompatibleConfigurationId).ThenBy(o => o.TemplateCode).ToList();
                Assert.Equal(2, compatibleTemplates.Count);
                Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());

                this.output.WriteLine($"Database configuration ids: {string.Join(", ", ids)}");
                this.output.WriteLine(
                    $"Compatible templates configuration ids: {string.Join(", ", compatibleTemplates.Select(t => t.CompatibleConfigurationId))}");

                Assert.Equal(ids[0], compatibleTemplates.Select(t => t.CompatibleConfigurationId).First());
                Assert.Equal(ids[1], compatibleTemplates.Select(t => t.CompatibleConfigurationId).Skip(1).First());
            }
        }

        /// <summary>
        /// Tests the configuration compatibility set-up - templates with changed modules versions are incompatible
        /// </summary>
        [Fact]
        public void TestConfigurationCompatibilityModuleVersionSet()
        {
            this.CreateTestDatabase();

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

            var configuration = new Configuration
                              {
                                  MinorVersion = 1,
                                  Name = "1",
                                  State = EnConfigurationState.Active,
                                  Settings =
                                      new ConfigurationSettings
                                          {
                                              NodeTemplates =
                                                  this.GetList(template1, template2),
                                              Packages = this.CreatePackages(
                                                  "p1;0.0.1",
                                                  "p2;0.0.1",
                                                  "p3;0.0.2")
                                          }
                              };

            using (var context = this.CreateContext())
            {
                // facepalm - https://github.com/aspnet/EntityFramework/issues/6872
                var ids = context.Configurations.Select(r => r.Id).OrderByDescending(id => id).ToList();

                var compatibleTemplates = configuration.GetCompatibleTemplates(context)
                    .OrderByDescending(t => t.CompatibleConfigurationId).ThenBy(o => o.TemplateCode).ToList();
                Assert.Equal(2, compatibleTemplates.Count);
                Assert.Equal("template1", compatibleTemplates.Select(t => t.TemplateCode).First());

                this.output.WriteLine($"Database configuration ids: {string.Join(", ", ids)}");
                this.output.WriteLine(
                    $"Compatible templates configuration ids: {string.Join(", ", compatibleTemplates.Select(t => t.CompatibleConfigurationId))}");

                Assert.Equal(ids[0], compatibleTemplates.Select(t => t.CompatibleConfigurationId).First());
                Assert.Equal(ids[1], compatibleTemplates.Select(t => t.CompatibleConfigurationId).Skip(1).First());
            }
        }

        /// <summary>
        /// Creates a new data context
        /// </summary>
        /// <returns>The data context</returns>
        private ConfigurationContext CreateContext()
        {
            var builder = new DbContextOptionsBuilder<ConfigurationContext>();
            builder.UseInMemoryDatabase("KlusterKite.NodeManager.Tests.ConfigurationTests");
            return new ConfigurationContext(builder.Options);
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
        /// Creates the database with test data
        /// </summary>
        private void CreateTestDatabase()
        {
            var configurationContext = this.CreateContext();
            configurationContext.ResetValueGenerators();
            configurationContext.Database.EnsureDeleted();
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

            using (var context = this.CreateContext())
            {
                var configuration1 = new Configuration
                                   {
                                       MinorVersion = 1,
                                       Name = "1",
                                       State = EnConfigurationState.Obsolete,
                                       Settings =
                                           new ConfigurationSettings
                                               {
                                                   NodeTemplates =
                                                       this.GetList(template1, template2),
                                                   Packages = this.CreatePackages(
                                                       "p1;0.0.1",
                                                       "p2;0.0.1",
                                                       "p3;0.0.1")
                                               }
                                   };
                context.Configurations.Add(configuration1);
                context.SaveChanges();
            }

            using (var context = this.CreateContext())
            {
                var oldConfiguration = context.Configurations.First();
                var activeConfiguration = new Configuration
                                        {
                                            MinorVersion = 2,
                                            Name = "active",
                                            State = EnConfigurationState.Active,
                                            Settings =
                                                new ConfigurationSettings
                                                    {
                                                        NodeTemplates =
                                                            this.GetList(
                                                                template1,
                                                                template2),
                                                        Packages = this.CreatePackages(
                                                            "p1;0.0.1",
                                                            "p2;0.0.1",
                                                            "p3;0.0.1")
                                                    }
                                        };

                context.Configurations.Add(activeConfiguration);
                activeConfiguration.CompatibleTemplatesBackward = new List<CompatibleTemplate>();
                activeConfiguration.CompatibleTemplatesBackward.Add(
                    new CompatibleTemplate { CompatibleConfigurationId = oldConfiguration.Id, TemplateCode = template1.Code });
                activeConfiguration.CompatibleTemplatesBackward.Add(
                    new CompatibleTemplate { CompatibleConfigurationId = oldConfiguration.Id, TemplateCode = template2.Code });
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