// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorTests.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="MigrationActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;

    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    using ClusterKit.Core;
    using ClusterKit.Core.TestKit;
    using ClusterKit.Data;
    using ClusterKit.Data.EF;
    using ClusterKit.Data.EF.Effort;
    using ClusterKit.NodeManager.Client.Messages.Migration;
    using ClusterKit.NodeManager.Client.MigrationStates;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Tests.Migrations;

    using NuGet;

    using Xunit;
    using Xunit.Abstractions;

    using Installer = ClusterKit.Core.TestKit.Installer;

    /// <summary>
    /// Testing the <see cref="MigrationActor"/>
    /// </summary>
    public class MigratorTests : BaseActorTest<MigratorTests.Configurator>
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The database name
        /// </summary>
        private readonly string databaseName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MigratorTests" /> class.
        /// </summary>
        /// <param name="output">
        ///     The output.
        /// </param>
        public MigratorTests(ITestOutputHelper output)
            : base(output)
        {
            this.connectionString = this.Sys.Settings.Config.GetString(NodeManagerActor.ConfigConnectionStringPath);
            this.databaseName = this.Sys.Settings.Config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the downgrade migration
        /// </summary>
        [Fact]
        public void MigrationDownGradeCheckTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                var nextRelease = context.Releases.First(r => r.Id == 2);

                nextRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first""
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";

                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                        ""second"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "second");
                this.CreateMigration();

                var actor = this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Source, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Downgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Source, state.TemplateStates[0].Migrators[0].Resources[0].Position);

                var resourceUpgrade = new ResourceUpgrade
                                          {
                                              TemplateCode = "migrator",
                                              MigratorTypeName =
                                                  "ClusterKit.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Ask<RequestAcknowledged>(new[] { resourceUpgrade }.ToList(), TimeSpan.FromSeconds(1));
                this.ExpectMsg<ProcessingTheRequest>();
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log[0] as MigrationOperation;
                Assert.NotNull(record);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(1, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("second", record.SourcePoint);
                Assert.Equal("first", record.DestinationPoint);
                Assert.Null(record.Error);

                state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Destination, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Downgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Destination, state.TemplateStates[0].Migrators[0].Resources[0].Position);
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the migration with no resource change
        /// </summary>
        [Fact]
        public void MigrationNoChangeTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                var nextRelease = context.Releases.First(r => r.Id == 2);

                nextRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first""
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";

                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");
                this.CreateMigration();

                this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.NoMigrationNeeded, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Stay, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(
                    EnResourcePosition.SourceAndDestination,
                    state.TemplateStates[0].Migrators[0].Resources[0].Position);
            }
            finally
            {
                File.Delete(resourceName);
            }
        }
        
        /// <summary>
        /// <see cref="MigrationActor"/> fails on get state request
        /// </summary>
        [Fact]
        public void MigrationCheckFailed()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                var nextRelease = context.Releases.First(r => r.Id == 2);

                nextRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first""
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";

                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]

                    TestMigrator.ThrowOnGetMigratableResources = true
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");
                this.CreateMigration();

                this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorInitializationFailed>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the upgrade migration
        /// </summary>
        [Fact]
        public void MigrationUpgradeCheckTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                var nextRelease = context.Releases.First(r => r.Id == 2);

                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first""
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";

                nextRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                        ""second"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");
                this.CreateMigration();

                var actor = this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(10));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Source, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Upgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Source, state.TemplateStates[0].Migrators[0].Resources[0].Position);

                var resourceUpgrade = new ResourceUpgrade
                                          {
                                              TemplateCode = "migrator",
                                              MigratorTypeName =
                                                  "ClusterKit.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Ask<RequestAcknowledged>(new[] { resourceUpgrade }.ToList(), TimeSpan.FromSeconds(1)); 
                this.ExpectMsg<ProcessingTheRequest>();
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log[0] as MigrationOperation;
                Assert.NotNull(record);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(2, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);
                Assert.Null(record.Error);

                state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Destination, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Upgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Destination, state.TemplateStates[0].Migrators[0].Resources[0].Position);
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the upgrade migration
        /// </summary>
        [Fact]
        public void  ReleaseUpgradeCheckTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                        ""second"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");

                var actor = this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                var state = this.ExpectMsg<MigrationActorReleaseState>(TimeSpan.FromSeconds(10));
                this.ExpectNoMsg();
                Assert.Equal(1, state.States.Count);
                Assert.Equal(1, state.States[0].MigratorsStates.Count);
                Assert.Equal(1, state.States[0].MigratorsStates[0].Resources.Count);
                Assert.Equal("first", state.States[0].MigratorsStates[0].Resources[0].CurrentPoint);

                var resourceUpgrade = new ResourceUpgrade
                                          {
                                              TemplateCode = "migrator",
                                              MigratorTypeName =
                                                  "ClusterKit.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Ask<RequestAcknowledged>(new[] { resourceUpgrade }.ToList(), TimeSpan.FromSeconds(1));
                this.ExpectMsg<ProcessingTheRequest>();
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log[0] as MigrationOperation;
                Assert.NotNull(record);
                Assert.Null(record.MigrationId);
                Assert.Equal(1, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);
                Assert.Null(record.Error);

                state = this.ExpectMsg<MigrationActorReleaseState>(TimeSpan.FromSeconds(10));
                this.ExpectNoMsg();
                Assert.Equal(1, state.States.Count);
                Assert.Equal(1, state.States[0].MigratorsStates.Count);
                Assert.Equal(1, state.States[0].MigratorsStates[0].Resources.Count);
                Assert.Equal("second", state.States[0].MigratorsStates[0].Resources[0].CurrentPoint);
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the upgrade migration
        /// </summary>
        [Fact]
        public void ReleaseCheckFailedTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                        ""second"",
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]

                    TestMigrator.ThrowOnGetMigratableResources = true
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");

                this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                this.ExpectMsg<MigrationActorInitializationFailed>(TimeSpan.FromSeconds(10));
                this.ExpectNoMsg();
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the upgrade migration with failed migration
        /// </summary>
        [Fact]
        public void MigrationUpgradeMigrationFailedTest()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var activeRelease = context.Releases.First(r => r.Id == 1);
                var nextRelease = context.Releases.First(r => r.Id == 2);

                activeRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first""
                    ]
                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]
                }}
                ";

                nextRelease.Configuration.MigratorTemplates.First().Configuration = $@"
                {{
                    TestMigrator.DefinedMigrationPoints = [
                        ""first"",
                        ""second"",
                    ]

                    TestMigrator.Resources = [
                        ""{resourceName.Replace("\\", "\\\\")}""
                    ]
                    ClusterKit.NodeManager.Migrators = [
                        ""ClusterKit.NodeManager.Tests.Migrations.TestMigrator, ClusterKit.NodeManager.Tests""
                    ]

                    TestMigrator.ThrowOnMigrate = true
                }}
                ";
                context.SaveChanges();
            }

            try
            {
                TestMigrator.SetMigrationPoint(resourceName, "first");
                this.CreateMigration();

                var actor = this.ActorOf(
                    () => new MigratorForwarder(
                        this.TestActor,
                        this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>(),
                        this.WindsorContainer.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(10));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Source, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Upgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Source, state.TemplateStates[0].Migrators[0].Resources[0].Position);

                var resourceUpgrade = new ResourceUpgrade
                                          {
                                              TemplateCode = "migrator",
                                              MigratorTypeName =
                                                  "ClusterKit.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Ask<RequestAcknowledged>(new[] { resourceUpgrade }.ToList(), TimeSpan.FromSeconds(1));
                this.ExpectMsg<ProcessingTheRequest>();
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log[0] as MigrationOperation;
                Assert.NotNull(record);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(2, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);
                Assert.NotNull(record.Error);
                Assert.Equal(1, record.Error.MigrationId);
                Assert.Equal(2, record.Error.ReleaseId);
                Assert.Equal("migrator", record.Error.MigratorTemplateCode);
                Assert.Equal("Exception while migrating resource: Migrate failed", record.Error.ErrorMessage);

                state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Source, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Upgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(EnResourcePosition.Source, state.TemplateStates[0].Migrators[0].Resources[0].Position);
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// Creates the new migration in database
        /// </summary>
        private void CreateMigration()
        {
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();
            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                var migration = new Migration
                                    {
                                        FromReleaseId = 1,
                                        ToReleaseId = 2,
                                        IsActive = true,
                                        State = EnMigrationState.Preparing
                                    };
                context.Migrations.Add(migration);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Creates test release
        /// </summary>
        /// <param name="repo">The package repository</param>
        /// <returns>The release</returns>
        private Release CreateRelease(IPackageRepository repo)
        {
            var template = new NodeTemplate
                               {
                                   Code = "node",
                                   Configuration = "{}",
                                   ContainerTypes = new[] { "node" }.ToList(),
                                   Name = "node",
                                   PackageRequirements =
                                       new[]
                                           {
                                               new NodeTemplate.PackageRequirement(
                                                   "ClusterKit.NodeManager",
                                                   null)
                                           }.ToList()
                               };

            var migrator = new MigratorTemplate
                               {
                                   Code = "migrator",
                                   Configuration = "{}",
                                   Name = "migrator",
                                   PackageRequirements =
                                       new[]
                                           {
                                               new NodeTemplate.PackageRequirement(
                                                   "ClusterKit.NodeManager.Tests",
                                                   null)
                                           }.ToList()
                               };

            var configuration = new ReleaseConfiguration
                                    {
                                        NodeTemplates = new[] { template }.ToList(),
                                        MigratorTemplates = new[] { migrator }.ToList(),
                                        Packages =
                                            repo.Search(string.Empty, true)
                                                .Select(
                                                    p => new PackageDescription
                                                             {
                                                                 Id = p.Id,
                                                                 Version =
                                                                     p.Version
                                                                         .ToString()
                                                             })
                                                .ToList(),
                                        NugetFeeds = new[] { new NugetFeed() }.ToList(),
                                        SeedAddresses = new[] { "http://seed" }.ToList()
                                    };

            var release = new Release { Configuration = configuration };
            return release;
        }

        /// <summary>
        /// Creates the test releases in test database
        /// </summary>
        private void CreateReleases()
        {
            var repo = this.WindsorContainer.Resolve<IPackageRepository>();
            var contextFactory = this.WindsorContainer.Resolve<IContextFactory<ConfigurationContext>>();

            using (var context = contextFactory.CreateContext(this.connectionString, this.databaseName).Result)
            {
                context.Database.Delete();
                context.Database.CreateIfNotExists();
                var activeRelease = this.CreateRelease(repo);
                context.Releases.Add(activeRelease);
                context.SaveChanges();
                var errors = activeRelease.CheckAll(context, repo, new[] { ReleaseCheckTestsBase.Net45 }.ToList())
                    .ToList();
                foreach (var error in errors)
                {
                    this.Sys.Log.Error("Error in active release {Field}: {Message}", error.Field, error.Message);
                }

                Assert.Equal(0, errors.Count);
                activeRelease.State = EnReleaseState.Active;
                context.SaveChanges();

                var nextRelease = this.CreateRelease(repo);
                context.Releases.Add(nextRelease);
                context.SaveChanges();
                errors = nextRelease.CheckAll(context, repo, new[] { ReleaseCheckTestsBase.Net45 }.ToList()).ToList();
                foreach (var error in errors)
                {
                    this.Sys.Log.Error("Error in next release {Field}: {Message}", error.Field, error.Message);
                }

                Assert.Equal(0, errors.Count);
                nextRelease.State = EnReleaseState.Ready;
                context.SaveChanges();
            }
        }

        /// <summary>
        ///     Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <summary>
            ///     Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers = new List<BaseInstaller> { new Installer(), new TestInstaller() };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// The overload for <see cref="MigrationActor"/>
        /// </summary>
        public class MigratorForwarder : MigrationActor
        {
            public MigratorForwarder(
                IActorRef testActor,
                IContextFactory<ConfigurationContext> contextFactory,
                IPackageRepository nugetRepository)
                : base(contextFactory, nugetRepository)
            {
                this.Parent = testActor;
            }

            /// <inheritdoc />
            protected override IActorRef Parent { get; }
        }

        /// <summary>
        ///     Replaces production data sources with the test ones
        /// </summary>
        private class TestInstaller : BaseInstaller
        {
            /// <inheritdoc />
            protected override decimal AkkaConfigLoadPriority => -1M;

            /// <inheritdoc />
            protected override Config GetAkkaConfig()
            {
                return ConfigurationFactory.ParseString(
                    $@"
            {{
                ClusterKit.NodeManager.ConfigurationDatabaseName = """"
                ClusterKit.NodeManager.ConfigurationDatabaseConnectionString = ""{Guid.NewGuid():N}""
                ClusterKit.NodeManager.FrameworkType = ""{ReleaseCheckTestsBase.Net45}""

                akka : {{
                  actor: {{
                    provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
                    deployment {{
                        /nodemanager {{
                            dispatcher = akka.test.calling-thread-dispatcher
                        }}
                        /nodemanager/workers {{
                            router = consistent-hashing-pool
                            nr-of-instances = 5
                            dispatcher = akka.test.calling-thread-dispatcher
                        }}
                    }}

                    serializers {{
		                hyperion = ""Akka.Serialization.HyperionSerializer, Akka.Serialization.Hyperion""
                    }}
                    serialization-bindings {{
                        ""System.Object"" = hyperion
                    }}
                  }}

                    remote : {{
                        helios.tcp : {{
                          hostname = 127.0.0.1
                          port = 0
                        }}
                      }}

                      cluster: {{
                        auto-down-unreachable-after = 15s
		                min-nr-of-members = 3
                        seed-nodes = []
                        singleton {{
                            min-number-of-hand-over-retries = 10
                        }}
                      }}
                }}
            }}");
            }

            /// <inheritdoc />
            protected override void RegisterWindsorComponents(IWindsorContainer container, IConfigurationStore store)
            {
                container.Register(
                    Classes.FromAssemblyContaining<NodeManagerActor>()
                        .Where(t => t.IsSubclassOf(typeof(ActorBase)))
                        .LifestyleTransient());
                container.Register(
                    Classes.FromAssemblyContaining<Core.Installer>()
                        .Where(t => t.IsSubclassOf(typeof(ActorBase)))
                        .LifestyleTransient());

                container.Register(
                    Component.For<BaseConnectionManager>().Instance(new ConnectionManager()).LifestyleSingleton());
                container.Register(
                    Component.For<IContextFactory<ConfigurationContext>>()
                        .ImplementedBy<EffortContextFactory<ConfigurationContext>>()
                        .LifestyleTransient());

                container.Register(
                    Component.For<DataFactory<ConfigurationContext, Release, int>>()
                        .ImplementedBy<ReleaseDataFactory>()
                        .LifestyleTransient());

                var packageRepository = this.CreateTestRepository();

                container.Register(Component.For<IPackageRepository>().Instance(packageRepository));
                container.Register(
                    Component.For<IMessageRouter>().ImplementedBy<TestMessageRouter>().LifestyleSingleton());
            }

            /// <summary>
            /// Creates test package from assembly
            /// </summary>
            /// <param name="assembly">The source assembly</param>
            /// <param name="allAssemblies">The list of all defined assemblies</param>
            /// <returns>The test package</returns>
            private ReleaseCheckTestsBase.TestPackage CreateTestPackage(Assembly assembly, Assembly[] allAssemblies)
            {
                Action<IFileSystem, string> extractContentsAction = (system, destination) =>
                    {
                        foreach (var f in assembly.GetFiles())
                        {
                            var fileName = Path.GetFileName(f.Name) ?? $"{assembly.GetName().Name}.dll";
                            system.AddFile(Path.Combine(destination, "lib", fileName), f);
                        }
                    };

                Func<IEnumerable<IPackageFile>> filesAction = () => assembly.GetFiles()
                    .Select(
                        fs => new ReleaseCheckTestsBase.TestPackageFile
                                  {
                                      EffectivePath =
                                          Path.Combine(
                                              "lib",
                                              Path.GetFileName(fs.Name)
                                              ?? fs.Name),
                                      GetStreamAction = () => fs,
                                      Path = Path.Combine(
                                          "lib",
                                          Path.GetFileName(fs.Name) ?? fs.Name)
                                  });

                var dependencies = assembly.GetReferencedAssemblies()
                    .Where(
                        d =>
                            {
                                var dependentAssembly = allAssemblies.FirstOrDefault(a => a.GetName().Name == d.Name);
                                return dependentAssembly != null && !dependentAssembly.IsDynamic
                                       && !dependentAssembly.GlobalAssemblyCache;
                            })
                    .Select(
                        d => new PackageDependency(
                            d.Name,
                            new VersionSpec(SemanticVersion.Parse(d.Version.ToString()))));

                return new ReleaseCheckTestsBase.TestPackage
                           {
                               Id = assembly.GetName().Name,
                               Version =
                                   new SemanticVersion(
                                       assembly.GetName().Version.ToString()),
                               DependencySets =
                                   new List<PackageDependencySet>
                                       {
                                           new
                                               PackageDependencySet(
                                                   null,
                                                   dependencies)
                                       },
                               ExtractContentsAction = extractContentsAction,
                               GetFilesAction = filesAction
                           };
            }

            /// <summary>
            ///     Creates the test repository
            /// </summary>
            /// <returns>The test repository</returns>
            private IPackageRepository CreateTestRepository()
            {
                var packages = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.GlobalAssemblyCache && !a.IsDynamic)
                    .Select(p => this.CreateTestPackage(p, AppDomain.CurrentDomain.GetAssemblies()));
                return new ReleaseCheckTestsBase.TestRepository(packages.Cast<IPackage>().ToArray());
            }
        }
    }
}