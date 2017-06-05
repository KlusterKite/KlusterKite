using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterKit.NodeManager.Tests
{
    using System.Diagnostics;
    using System.IO;

    using Akka.Configuration;

    using Castle.Facilities.TypedFactory;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.ConfigurationSource.Seeder;
    using ClusterKit.NodeManager.Launcher.Utils;

    using Microsoft.EntityFrameworkCore;

    using Npgsql;

    using NuGet;

    using Xunit;
    using Xunit.Abstractions;

    public class TempTest
    {
        private readonly ITestOutputHelper output;

        /// <summary>
        /// Initializes a new instance of the <see cref="TempTest"/> class.
        /// </summary>
        /// <param name="output">
        /// The output.
        /// </param>
        public TempTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void MigratorTest()
        {
            //User ID=postgres;Host=docker;Port=5432;Database=clusterkit.nodemanager.configuration;Pooling=true
            var connectionStringBuilder =
                new NpgsqlConnectionStringBuilder("User ID=postgres;Host=docker;Port=5432;Pooling=true")
                    {
                        Database = Guid.NewGuid().ToString("N")
                    };

            var connectionString = connectionStringBuilder.ConnectionString;
            var optionsBuilder = new DbContextOptionsBuilder<ConfigurationContext>();
            optionsBuilder.UseNpgsql(connectionString);

            using (var context = new ConfigurationContext(optionsBuilder.Options))
            {
                try
                {
                    context.Database.Migrate();
                    var release = new Release();
                    context.Releases.Add(release);
                    context.SaveChanges();
                }
                finally
                {
                    context.Database.EnsureDeleted();
                }
            }
        }

        [Fact]
        public void NugetTest()
        {
            var packages = PackageUtils.Search("http://nuget/", string.Empty).GetAwaiter().GetResult()
                .OrderBy(p => p.Identity.Id).ToList();
            foreach (var package in packages)
            {
                this.output.WriteLine($"{package.Identity.Id} {string.Join(", ", package.GetVersionsAsync().GetAwaiter().GetResult().Select(v => v.Version))}");
            }
        }

        [Fact]
        public void SeederTest()
        {
            var config = ConfigurationFactory.ParseString(File.ReadAllText("C:\\Dropbox\\Sources\\Git\\ClusterKit\\Docker\\ClusterKitSeeder\\seeder.hocon "));
            var container = new WindsorContainer();
            container.AddFacility<TypedFactoryFacility>();
            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel, true));
            container.Register(Component.For<IWindsorContainer>().Instance(container));
            container.RegisterWindsorInstallers();
            config = BaseInstaller.GetStackedConfig(container, config);
            container.Register(Component.For<Config>().Instance(config));

            var seeder = new TestSeeder(config, container.Resolve<UniversalContextFactory>(), this.output);
            seeder.Seed();
            
        }

        protected class TestSeeder : Seeder
        {
            private readonly ITestOutputHelper output;

            /// <inheritdoc />
            public TestSeeder(Config config, UniversalContextFactory contextFactory, ITestOutputHelper output)
                : base(config, contextFactory)
            {
                this.output = output;
            }

            /// <inheritdoc />
            public override void Seed()
            {
                var repository = this.Config.GetString("Nuget");
                var watch = new Stopwatch();
                watch.Start();
                var packageDescriptions = this.GetPackageDescriptions(repository).GetAwaiter().GetResult();
                watch.Stop();
                this.output.WriteLine($"Retrieved packages in {watch.ElapsedMilliseconds}ms");
                watch.Restart();
                var configuration =
                    new ReleaseConfiguration
                        {
                            NodeTemplates = this.GetNodeTemplates().ToList(),
                            MigratorTemplates = this.GetMigratorTemplates().ToList(),
                            Packages = packageDescriptions,
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

                var supportedFrameworks = this.Config.GetStringList("ClusterKit.NodeManager.SupportedFrameworks");
                watch.Stop();
                this.output.WriteLine($"Release created in {watch.ElapsedMilliseconds}ms");
                watch.Restart();

                var initialErrors =
                    initialRelease.SetPackagesDescriptionsForTemplates(repository, supportedFrameworks.ToList()).GetAwaiter().GetResult();

                watch.Stop();
                this.output.WriteLine($"Templates set in {watch.ElapsedMilliseconds}ms");

                foreach (var errorDescription in initialErrors)
                {
                    this.output.WriteLine($"ERROR: {errorDescription}");
                }

                foreach (var packageDescription in initialRelease.Configuration.MigratorTemplates.First().PackagesToInstall.Values.First().OrderBy(p => p.Id))
                {
                    this.output.WriteLine($"{packageDescription.Id} {packageDescription.Version}");
                }
            }
        }
    }
}
