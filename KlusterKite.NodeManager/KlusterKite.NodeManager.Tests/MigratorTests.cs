// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorTests.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Testing the <see cref="MigrationActor" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    using Akka.Actor;
    using Akka.Configuration;

    using Autofac;

    using KlusterKite.Core;
    using KlusterKite.Core.TestKit;
    using KlusterKite.Data;
    using KlusterKite.Data.EF;
    using KlusterKite.Data.EF.InMemory;
    using KlusterKite.NodeManager.Client.Messages.Migration;
    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.ConfigurationSource;
    using KlusterKite.NodeManager.Launcher.Messages;
    using KlusterKite.NodeManager.Launcher.Utils;
    using KlusterKite.NodeManager.Tests.Migrations;

#if CORECLR
    using Microsoft.Extensions.DependencyModel;
#endif

    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Versioning;

    using Xunit;
    using Xunit.Abstractions;

    using Installer = KlusterKite.Core.TestKit.Installer;

    /// <summary>
    /// Testing the <see cref="MigrationActor"/>
    /// </summary>
    [Collection("KlusterKite.NodeManager.Tests.ConfigurationContext")]
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
        /// <see cref="MigrationActor"/> fails on get state request
        /// </summary>
        [Fact]
        public void MigrationCheckFailed()
        {
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                this.ExpectMsg<MigrationActorInitializationFailed>(TimeSpan.FromSeconds(30));
                this.ExpectNoMsg();
            }
            finally
            {
                File.Delete(resourceName);
            }
        }

        /// <summary>
        /// <see cref="MigrationActor"/> checks the downgrade migration
        /// </summary>
        [Fact]
        public void MigrationDownGradeCheckTest()
        {
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(30));
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
                                                  "KlusterKite.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Tell(new[] { resourceUpgrade }.ToList());
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log.First();
                Assert.Equal(EnMigrationLogRecordType.Operation, record.Type);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(1, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("second", record.SourcePoint);
                Assert.Equal("first", record.DestinationPoint);

                state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Destination, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Downgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(
                    EnResourcePosition.Destination,
                    state.TemplateStates[0].Migrators[0].Resources[0].Position);
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
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(30));
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
        /// <see cref="MigrationActor"/> checks the upgrade migration
        /// </summary>
        [Fact]
        public void MigrationUpgradeCheckTest()
        {
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(30));
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
                                                  "KlusterKite.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Tell(new[] { resourceUpgrade }.ToList());
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log.First();
                Assert.Equal(EnMigrationLogRecordType.Operation, record.Type);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(2, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);

                state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(5));
                this.ExpectNoMsg();
                Assert.Equal(EnMigrationActorMigrationPosition.Destination, state.Position);
                Assert.Equal(1, state.TemplateStates.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Position);
                Assert.Equal(1, state.TemplateStates[0].Migrators.Count);
                Assert.Equal(EnMigratorPosition.Merged, state.TemplateStates[0].Migrators[0].Position);
                Assert.Equal(EnMigrationDirection.Upgrade, state.TemplateStates[0].Migrators[0].Direction);
                Assert.Equal(1, state.TemplateStates[0].Migrators[0].Resources.Count);
                Assert.Equal(
                    EnResourcePosition.Destination,
                    state.TemplateStates[0].Migrators[0].Resources[0].Position);
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
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();

                var state = this.ExpectMsg<MigrationActorMigrationState>(TimeSpan.FromSeconds(30));
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
                                                  "KlusterKite.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Tell(new[] { resourceUpgrade }.ToList());
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log.First();
                Assert.Equal(EnMigrationLogRecordType.OperationError, record.Type);
                Assert.Equal(1, record.MigrationId);
                Assert.Equal(2, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);
                Assert.Equal("Exception while migrating resource: Migrate failed", record.ErrorMessage);

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
        /// <see cref="MigrationActor"/> checks the upgrade migration
        /// </summary>
        [Fact]
        public void ReleaseCheckFailedTest()
        {
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                this.ExpectMsg<MigrationActorInitializationFailed>(TimeSpan.FromSeconds(30));
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
        public void ReleaseUpgradeCheckTest()
        {
            this.CreateReleases();
            var resourceName = Path.Combine(Path.GetFullPath("."), Guid.NewGuid().ToString("N"));
            using (var context = this.GetContext())
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
                    KlusterKite.NodeManager.Migrators = [
                        ""KlusterKite.NodeManager.Tests.Migrations.TestMigrator, KlusterKite.NodeManager.Tests""
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
                        this.Container.Resolve<UniversalContextFactory>(),
                        this.Container.Resolve<IPackageRepository>()),
                    "migrationActor");
                this.ExpectMsg<ProcessingTheRequest>();
                var state = this.ExpectMsg<MigrationActorReleaseState>(TimeSpan.FromSeconds(45));
                this.ExpectNoMsg();
                Assert.Equal(1, state.States.Count);
                Assert.Equal(1, state.States[0].MigratorsStates.Count);
                Assert.Equal(1, state.States[0].MigratorsStates[0].Resources.Count);
                Assert.Equal("first", state.States[0].MigratorsStates[0].Resources[0].CurrentPoint);

                var resourceUpgrade = new ResourceUpgrade
                                          {
                                              TemplateCode = "migrator",
                                              MigratorTypeName =
                                                  "KlusterKite.NodeManager.Tests.Migrations.TestMigrator",
                                              ResourceCode = Path.GetFileName(resourceName),
                                              Target = EnMigrationSide.Destination
                                          };

                actor.Ask<RequestAcknowledged>(new[] { resourceUpgrade }.ToList(), TimeSpan.FromSeconds(1));
                this.ExpectMsg<ProcessingTheRequest>();
                var log = this.ExpectMsg<List<MigrationLogRecord>>();
                Assert.Equal(1, log.Count);
                var record = log.First();
                Assert.Equal(EnMigrationLogRecordType.Operation, record.Type);
                Assert.Null(record.MigrationId);
                Assert.Equal(1, record.ReleaseId);
                Assert.Equal("migrator", record.MigratorTemplateCode);
                Assert.Equal("first", record.SourcePoint);
                Assert.Equal("second", record.DestinationPoint);

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
        /// Creates the new migration in database
        /// </summary>
        private void CreateMigration()
        {
            using (var context = this.GetContext())
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
                                                   "KlusterKite.NodeManager",
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
                                                   "KlusterKite.NodeManager.Tests",
                                                   null),
                                               new NodeTemplate.PackageRequirement(
                                                   "Akka.Logger.Serilog",
                                                   null),
                                           }.ToList()
                               };

            var packageDescriptions = repo.SearchAsync(string.Empty, true).GetAwaiter().GetResult()
                .Select(p => p.Identity).Select(
                    p => new PackageDescription { Id = p.Id, Version = p.Version.ToString() }).ToList();

            var configuration = new ReleaseConfiguration
                                    {
                                        NodeTemplates = new[] { template }.ToList(),
                                        MigratorTemplates = new[] { migrator }.ToList(),
                                        Packages = packageDescriptions,
                                        NugetFeed = "http://nuget/",
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
            var repo = this.Container.Resolve<IPackageRepository>();

            using (var context = this.GetContext())
            {
                var activeRelease = this.CreateRelease(repo);
                context.Releases.Add(activeRelease);
                context.SaveChanges();
                var errors = activeRelease.CheckAll(context, repo, new[] { ReleaseCheckTestsBase.Net46, ReleaseCheckTestsBase.NetCore }.ToList())
                    .GetAwaiter().GetResult().ToList();
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
                errors = nextRelease.CheckAll(context, repo, new[] { ReleaseCheckTestsBase.Net46, ReleaseCheckTestsBase.NetCore }.ToList())
                    .GetAwaiter().GetResult().ToList();
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
        ///     Creates database context
        /// </summary>
        /// <returns>The database context</returns>
        private ConfigurationContext GetContext()
        {
            return this.Container.Resolve<UniversalContextFactory>()
                .CreateContext<ConfigurationContext>("InMemory", this.connectionString, this.databaseName);
        }

        /// <summary>
        ///     Configures current test system
        /// </summary>
        public class Configurator : TestConfigurator
        {
            /// <inheritdoc />
            public override bool RunPostStart => true;

            /// <summary>
            ///     Gets list of all used plugin installers
            /// </summary>
            /// <returns>The list of installers</returns>
            public override List<BaseInstaller> GetPluginInstallers()
            {
                var pluginInstallers =
                    new List<BaseInstaller>
                        {
                            new Installer(),
                            new TestInstaller(),
                            new Data.Installer(),
                            new Data.EF.Installer(),
                            new Data.EF.InMemory.Installer()
                        };
                return pluginInstallers;
            }
        }

        /// <summary>
        /// The overload for <see cref="MigrationActor"/>
        /// </summary>
        private class MigratorForwarder : MigrationActor
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MigratorForwarder"/> class.
            /// </summary>
            /// <param name="testActor">The test actor reference</param>
            /// <param name="contextFactory">The context factory</param>
            /// <param name="nugetRepository">The nuget repository</param>
            public MigratorForwarder(
                IActorRef testActor,
                UniversalContextFactory contextFactory,
                IPackageRepository nugetRepository)
                : base(contextFactory, nugetRepository)
            {
                this.Parent = testActor;
            }

            /// <inheritdoc />
            protected override IActorRef Parent { get; }
        }

        /// <summary>
        /// Replaces production data sources with the test ones
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
                KlusterKite.NodeManager.ConfigurationDatabaseName = ""{Guid.NewGuid():N}""
                KlusterKite.NodeManager.ConfigurationDatabaseProviderName = ""InMemory""
                KlusterKite.NodeManager.ConfigurationDatabaseConnectionString = """"
                KlusterKite.NodeManager.FrameworkType = ""{ReleaseCheckTestsBase.Net46}""

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
            protected override void PostStart(IComponentContext componentContext)
            {
                var contextManager = componentContext.Resolve<UniversalContextFactory>();
                var config = componentContext.Resolve<Config>();
                var connectionString = config.GetString(NodeManagerActor.ConfigConnectionStringPath);
                var databaseName = config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
                using (var context =
                    contextManager.CreateContext<ConfigurationContext>("InMemory", connectionString, databaseName))
                {
                    context.ResetValueGenerators();
                    context.Database.EnsureDeleted();
                }
            }

            /// <inheritdoc />
            protected override void RegisterComponents(ContainerBuilder container, Config config)
            {
                container.RegisterAssemblyTypes(typeof(NodeManagerActor).GetTypeInfo().Assembly)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));
                container.RegisterAssemblyTypes(typeof(Core.Installer).GetTypeInfo().Assembly)
                    .Where(t => t.GetTypeInfo().IsSubclassOf(typeof(ActorBase)));

                container.RegisterType<ReleaseDataFactory>().As<DataFactory<ConfigurationContext, Release, int>>();

                container.RegisterInstance(this.CreateTestRepository()).As<IPackageRepository>();
                container.RegisterType<TestMessageRouter>().As<IMessageRouter>();
            }

            /// <summary>
            /// Gets the list of loaded assemblies
            /// </summary>
            /// <returns>The list of loaded assemblies</returns>
            private static IEnumerable<Assembly> GetLoadedAssemblies()
            {
#if APPDOMAIN
                var assemblies = new List<Assembly>();
                var currentDirectory = Path.GetFullPath(".");
                foreach (var file in Directory.GetFiles(currentDirectory, "*.dll"))
                {
                    try
                    {
                        assemblies.Add(Assembly.ReflectionOnlyLoadFrom(file));
                    }
                    catch
                    {
                        // ignore
                    }
                }

                foreach (var file in Directory.GetFiles(currentDirectory, "*.exe"))
                {
                    try
                    {
                        assemblies.Add(Assembly.ReflectionOnlyLoadFrom(file));
                    }
                    catch
                    {
                        // ignore
                    }
                }

                return assemblies;
#elif CORECLR
                var assemblies = new List<Assembly>();
                var dependencies = DependencyContext.Default.RuntimeLibraries;
                foreach (var library in dependencies)
                {
                    try
                    {
                        var assembly = Assembly.Load(new AssemblyName(library.Name));
                        assemblies.Add(assembly);
                    }
                    catch
                    {
                        // do nothing can't if can't load assembly
                    }
                }

                return assemblies;
#endif
            }

            /// <summary>
            /// Creates test package from assembly
            /// </summary>
            /// <param name="assembly">The source assembly</param>
            /// <param name="allAssemblies">The list of all defined assemblies</param>
            /// <returns>The test package</returns>
            private ReleaseCheckTestsBase.TestPackage CreateTestPackage(Assembly assembly, Assembly[] allAssemblies)
            {
                /*
                Action<IFileSystem, string> extractContentsAction = (system, destination) =>
                    {
                        foreach (var f in assembly.GetFiles())
                        {
                            var fileName = Path.GetFileName(f.Name) ?? $"{assembly.GetName().Name}.dll";
                            system.AddFile(Path.Combine(destination, "lib", fileName), f);
                        }
                    };
                    */

                /*
                Func<IEnumerable<IPackageFile>> filesAction = () => assembly.GetFiles().Select(
                    fs => new ReleaseCheckTestsBase.TestPackageFile
                              {
                                  EffectivePath =
                                      Path.Combine(
                                          "lib",
                                          Path.GetFileName(fs.Name) ?? fs.Name),
                                  GetStreamAction = () => fs,
                                  Path = Path.Combine(
                                      "lib",
                                      Path.GetFileName(fs.Name) ?? fs.Name)
                              });
                              */
                var dependencies = assembly.GetReferencedAssemblies().Select(
                    d =>
                        {
                            var dependentAssembly = allAssemblies.FirstOrDefault(a => a.GetName().Name == d.Name);
                            return dependentAssembly != null && !dependentAssembly.IsDynamic
#if APPDOMAIN
                                   && !dependentAssembly.GlobalAssemblyCache 
#endif
                                       ? dependentAssembly
                                       : null;
                        }).Where(d => d != null).Select(
                    d => new PackageDependency(
                        d.GetName().Name,
                        new VersionRange(NuGetVersion.Parse(d.GetName().Version.ToString())))).ToList();

                var standardDependencies = new PackageDependencyGroup(
                    NuGetFramework.ParseFrameworkName(
                        ReleaseCheckTestsBase.NetCore,
                        DefaultFrameworkNameProvider.Instance),
                    dependencies);
                var net46Dependencies = new PackageDependencyGroup(
                    NuGetFramework.ParseFrameworkName(
                        ReleaseCheckTestsBase.Net46,
                        DefaultFrameworkNameProvider.Instance),
                    dependencies);

                Func<string, string, string, IEnumerable<string>> extaction = (framework, destination, temp) =>
                    {
                        if (string.IsNullOrWhiteSpace(assembly.Location))
                        {
                            throw new InvalidOperationException("Assembly has no location");
                        }

                        var fileName = Path.GetFileName(assembly.Location);
                        File.Copy(assembly.Location, Path.Combine(destination, fileName));
                        return new[] { fileName };
                    };

                return new ReleaseCheckTestsBase.TestPackage(
                           assembly.GetName().Name,
                           assembly.GetName().Version.ToString())
                           {
                               DependencySets =
                                   new[]
                                       {
                                           standardDependencies,
                                           net46Dependencies
                                       },
                               Extract = extaction
                };
            }

            /// <summary>
            ///     Creates the test repository
            /// </summary>
            /// <returns>The test repository</returns>
            private IPackageRepository CreateTestRepository()
            {
                var loadedAssemblies = GetLoadedAssemblies()
#if APPDOMAIN
                    .Where(a => !a.GlobalAssemblyCache && !a.IsDynamic)
#elif CORECLR
                    .Where(a => !a.IsDynamic)
#endif                    
                    .ToList();

                var ignoredAssemblies = loadedAssemblies.SelectMany(a => a.GetReferencedAssemblies())
                    .Where(r => loadedAssemblies.All(a => a.FullName != r.FullName)).Select(r => r.FullName).ToList();

                var assemblies = loadedAssemblies.ToArray();
                var packages = assemblies
                    .Where(a => !ignoredAssemblies.Contains(a.FullName))
                    .Select(p => this.CreateTestPackage(p, assemblies)).GroupBy(a => a.Identity.Id)
                    .Select(g => g.OrderByDescending(a => a.Identity.Id).First()).ToArray();
                return new ReleaseCheckTestsBase.TestRepository(packages);
            }
        }
    }
}