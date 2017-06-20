// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The actor responsible for non-code cluster migrations and updates
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Akka;
    using Akka.Actor;
    using Akka.Event;

    using ClusterKit.Data.EF;
    using ClusterKit.NodeManager.Client.Messages.Migration;
    using ClusterKit.NodeManager.Client.MigrationStates;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.ConfigurationSource;
    using ClusterKit.NodeManager.Launcher.Messages;
    using ClusterKit.NodeManager.Launcher.Utils;
    using ClusterKit.NodeManager.Migrator;
    using ClusterKit.NodeManager.RemoteDomain;

    using Google.Protobuf;

    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    using Newtonsoft.Json;

    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;

    /// <summary>
    /// The actor responsible for non-code cluster migrations and updates
    /// </summary>
    public class MigrationActor : FSM<MigrationActor.EnState, MigrationActor.Data>
    {
        /// <summary>
        /// The configuration database connection string
        /// </summary>
        private readonly string connectionString;

        /// <summary>
        /// The context factory
        /// </summary>
        private readonly UniversalContextFactory contextFactory;

        /// <summary>
        /// The configuration database name
        /// </summary>
        private readonly string databaseName;

        /// <summary>
        /// The configuration database provider name
        /// </summary>
        private readonly string databaseProviderName;

        /// <summary>
        /// The current environment framework name
        /// </summary>
        private readonly string frameworkName;

        /// <summary>
        /// The nuget repository
        /// </summary>
        private readonly IPackageRepository nugetRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationActor"/> class.
        /// </summary>
        /// <param name="contextFactory">
        /// The context Factory.
        /// </param>
        /// <param name="nugetRepository">
        /// The nuget Repository.
        /// </param>
        [UsedImplicitly]
        public MigrationActor(UniversalContextFactory contextFactory, IPackageRepository nugetRepository)
        {
            this.Parent = Context.Parent;
            this.contextFactory = contextFactory;
            this.nugetRepository = nugetRepository;
            this.connectionString =
                Context.System.Settings.Config.GetString(NodeManagerActor.ConfigConnectionStringPath);
            this.databaseName = Context.System.Settings.Config.GetString(NodeManagerActor.ConfigDatabaseNamePath);
            this.databaseProviderName = Context.System.Settings.Config.GetString(NodeManagerActor.ConfigDatabaseProviderNamePath);
            this.frameworkName = Context.System.Settings.Config.GetString("ClusterKit.NodeManager.FrameworkType");
        }

        /// <summary>
        /// The state of migration
        /// </summary>
        public enum EnState
        {
            /// <summary>
            /// There is no migration in process
            /// </summary>
            Idle,

            /// <summary>
            /// There is migration in process
            /// </summary>
            Migration,

            /// <summary>
            /// The state checked failed
            /// </summary>
            InitializationFailed
        }

        /// <summary>
        /// Gets the parent actor to notify of action complete
        /// </summary>
        protected virtual IActorRef Parent { get; }

        /// <inheritdoc />
        protected override void PreStart()
        {
            this.When(
                EnState.Migration,
                command => command.FsmEvent.Match<State<EnState, Data>>()
                    .With<List<ResourceUpgrade>>(this.StateMigrationHandleResourceUpgrade)
                    .ResultOrDefault(o => null));

            this.When(
                EnState.Idle,
                command => command.FsmEvent.Match<State<EnState, Data>>()
                    .With<List<ResourceUpgrade>>(this.StateIdleHandleResourceUpgrade)
                    .ResultOrDefault(o => null));

            this.When(EnState.InitializationFailed, command => null);

            this.WhenUnhandled(
                command => command.FsmEvent.Match<State<EnState, Data>>()
                    .With<RecheckState>(m => this.LoadState())
                    .ResultOrDefault(
                        o =>
                            {
                                Context.GetLogger()
                                    .Warning(
                                    "{Type}: received unsupported message {MessageType} in state {StateName}",
                                    this.GetType().Name,
                                    o.GetType().Name,
                                    this.StateName.ToString());
                                return this.Stay();
                            }));

            this.Parent.Tell(new ProcessingTheRequest());
            var startState = this.LoadState(new Data(), true);
            this.StartWith(startState.StateName, startState.StateData, startState.Timeout);
            this.Initialize();
        }

        /// <summary>
        /// Creates the resource migration plan
        /// </summary>
        /// <param name="request">The list of migrating resources</param>
        /// <param name="errors">The list of processing errors</param>
        /// <returns>The migration plan</returns>
        private MigrationPlan CreateMigrationPlan(List<ResourceUpgrade> request, out List<MigrationError> errors)
        {
            var plan = new MigrationPlan();
            errors = new List<MigrationError>();
            var errorId = -1;
            foreach (var resourceUpgrade in request)
            {
                var error = new MigrationError
                                {
                                    Id = errorId--,
                                    Created = DateTimeOffset.Now,
                                    ReleaseId = this.StateData.Migration.ToReleaseId,
                                    MigratorTemplateCode = resourceUpgrade.TemplateCode,
                                    MigratorTypeName = resourceUpgrade.MigratorTypeName,
                                    ResourceCode = resourceUpgrade.ResourceCode
                                };

                var template =
                    this.StateData.MigrationState?.TemplateStates.FirstOrDefault(
                        t => t.Code == resourceUpgrade.TemplateCode);
                if (template == null)
                {
                    error.ErrorMessage = "Migrator template was not found";
                    errors.Add(error);
                    continue;
                }

                error.MigratorTemplateName = template.DestinationTemplate?.Name ?? template.SourceTemplate?.Name;

                var migrator = template.Migrators.FirstOrDefault(m => m.TypeName == resourceUpgrade.MigratorTypeName);
                if (migrator == null)
                {
                    error.ErrorMessage = "Migrator was not found";
                    errors.Add(error);
                    continue;
                }

                error.MigratorName = migrator.Name;

                var resource = migrator.Resources.FirstOrDefault(r => r.Code == resourceUpgrade.ResourceCode);
                if (resource == null)
                {
                    error.ErrorMessage = "Resource was not found";
                    errors.Add(error);
                    continue;
                }

                error.ResourceName = resource.Name;

                var executionSide = resourceUpgrade.Target == EnMigrationSide.Destination
                                        ? resource.MigrationToDestinationExecutor
                                        : resource.MigrationToSourceExecutor;

                if (!executionSide.HasValue)
                {
                    error.ErrorMessage = "Resource can not be migrated";
                    errors.Add(error);
                    continue;
                }

                var sideExecutionPlans = executionSide.Value == EnMigrationSide.Source
                                             ? plan.SourceExecution
                                             : plan.DestinationExecution;

                MigratorTemplatePlan templatePlan;
                if (!sideExecutionPlans.TryGetValue(resourceUpgrade.TemplateCode, out templatePlan))
                {
                    templatePlan = new MigratorTemplatePlan();
                    templatePlan.Template = executionSide.Value == EnMigrationSide.Source
                                                ? template.SourceTemplate
                                                : template.DestinationTemplate;
                    sideExecutionPlans[resourceUpgrade.TemplateCode] = templatePlan;
                }

                MigratorMigrationsCommand migratorMigrationsCommand;
                if (!templatePlan.MigratorPlans.TryGetValue(
                        resourceUpgrade.MigratorTypeName,
                        out migratorMigrationsCommand))
                {
                    migratorMigrationsCommand =
                        new MigratorMigrationsCommand { TypeName = resourceUpgrade.MigratorTypeName };
                    templatePlan.MigratorPlans[resourceUpgrade.MigratorTypeName] = migratorMigrationsCommand;
                }

                var migrationPoint = resourceUpgrade.Target == EnMigrationSide.Source
                                         ? resource.SourcePoint
                                         : resource.DestinationPoint;

                migratorMigrationsCommand.Resources.Add(resource.Code, migrationPoint);
            }

            return plan;
        }

        /// <summary>
        /// Creates migration state from release states
        /// </summary>
        /// <param name="sourceMigratorTemplateStates">The resource state according to source release</param>
        /// <param name="destinationMigratorTemplateStates">The resource state according to destination release</param>
        /// <returns>The overall resource migration state</returns>
        private IEnumerable<MigratorTemplateMigrationState> CreateMigrationState(
            IReadOnlyCollection<MigratorTemplateReleaseState> sourceMigratorTemplateStates,
            IReadOnlyCollection<MigratorTemplateReleaseState> destinationMigratorTemplateStates)
        {
            foreach (var destinationMigratorTemplateState in destinationMigratorTemplateStates)
            {
                var sourceMigratorTemplateState = sourceMigratorTemplateStates.FirstOrDefault(
                    t => t.Template.Code == destinationMigratorTemplateState.Template.Code);

                if (sourceMigratorTemplateState == null)
                {
                    yield return MigratorTemplateMigrationState.CreateFrom(
                        destinationMigratorTemplateState,
                        EnMigratorPosition.New);
                    continue;
                }

                var migratorStates = this.CreateMigrationState(
                    sourceMigratorTemplateState.MigratorsStates,
                    destinationMigratorTemplateState.MigratorsStates);

                yield return new MigratorTemplateMigrationState
                                 {
                                     Code =
                                         destinationMigratorTemplateState.Template.Code,
                                     DestinationTemplate =
                                         destinationMigratorTemplateState.Template,
                                     SourceTemplate =
                                         sourceMigratorTemplateState.Template,
                                     Position = EnMigratorPosition.Merged,
                                     Migrators = migratorStates.ToList()
                                 };
            }

            foreach (var sourceMigratorTemplateState in sourceMigratorTemplateStates.Where(
                s => destinationMigratorTemplateStates.All(d => d.Template.Code != s.Template.Code)))
            {
                yield return MigratorTemplateMigrationState.CreateFrom(
                    sourceMigratorTemplateState,
                    EnMigratorPosition.Obsolete);
            }
        }

        /// <summary>
        /// Creates migration state from release states
        /// </summary>
        /// <param name="sourceMigratorStates">The resource state according to source release</param>
        /// <param name="destinationMigratorStates">The resource state according to destination release</param>
        /// <returns>The overall resource migration state</returns>
        private IEnumerable<MigratorMigrationState> CreateMigrationState(
            IReadOnlyCollection<MigratorReleaseState> sourceMigratorStates,
            IReadOnlyCollection<MigratorReleaseState> destinationMigratorStates)
        {
            foreach (var destinationMigratorState in destinationMigratorStates)
            {
                var sourceMigratorState =
                    sourceMigratorStates.FirstOrDefault(s => s.TypeName == destinationMigratorState.TypeName);

                if (sourceMigratorState == null)
                {
                    yield return MigratorMigrationState.CreateFrom(destinationMigratorState, EnMigratorPosition.New);
                    continue;
                }

                var resourceStates = this.CreateMigrationState(sourceMigratorState, destinationMigratorState).ToList();

                var direction = this.GetDirection(
                    sourceMigratorState.MigrationPoints,
                    destinationMigratorState.MigrationPoints);

                yield return new MigratorMigrationState
                                 {
                                     Name = destinationMigratorState.Name,
                                     TypeName = destinationMigratorState.TypeName,
                                     Direction = direction,
                                     Position = EnMigratorPosition.Merged,
                                     Resources = resourceStates
                                 };
            }

            foreach (var sourceMigratorState in sourceMigratorStates.Where(
                s => destinationMigratorStates.All(d => d.TypeName != s.TypeName)))
            {
                yield return MigratorMigrationState.CreateFrom(sourceMigratorState, EnMigratorPosition.Obsolete);
            }
        }

        /// <summary>
        /// Creates migration state from release states
        /// </summary>
        /// <param name="sourceMigratorReleaseState">
        /// The migrator state according to source release
        /// </param>
        /// <param name="destinationMigratorReleaseState">
        /// The migrator state according to destination release
        /// </param>
        /// <returns>
        /// The overall resource migration state
        /// </returns>
        private IEnumerable<ResourceMigrationState> CreateMigrationState(
            MigratorReleaseState sourceMigratorReleaseState,
            MigratorReleaseState destinationMigratorReleaseState)
        {
            foreach (var destinationResourceState in destinationMigratorReleaseState.Resources)
            {
                var sourceResourceState =
                    sourceMigratorReleaseState.Resources.FirstOrDefault(s => s.Code == destinationResourceState.Code);

                if (sourceResourceState == null)
                {
                    yield return ResourceMigrationState.CreateFrom(
                        destinationMigratorReleaseState,
                        destinationResourceState,
                        EnMigratorPosition.New);
                    continue;
                }

                yield return ResourceMigrationState.CreateFrom(
                    sourceMigratorReleaseState,
                    sourceResourceState,
                    destinationMigratorReleaseState,
                    destinationResourceState);
            }

            foreach (var sourceResourceState in sourceMigratorReleaseState.Resources.Where(
                s => destinationMigratorReleaseState.Resources.All(d => d.Code != s.Code)))
            {
                yield return ResourceMigrationState.CreateFrom(
                    sourceMigratorReleaseState,
                    sourceResourceState,
                    EnMigratorPosition.Obsolete);
            }
        }

        /// <summary>
        /// Creates the resource migration plan
        /// </summary>
        /// <param name="request">The list of migrating resources</param>
        /// <param name="errors">The list of processing errors</param>
        /// <returns>The migration plan</returns>
        private MigrationPlan CreateReleasePlan(List<ResourceUpgrade> request, out List<MigrationError> errors)
        {
            var plan = new MigrationPlan();
            errors = new List<MigrationError>();
            foreach (var resourceUpgrade in request)
            {
                var error = new MigrationError
                                {
                                    Created = DateTimeOffset.Now,
                                    ReleaseId = this.StateData.Release.Id,
                                    MigratorTemplateCode = resourceUpgrade.TemplateCode,
                                    MigratorTypeName = resourceUpgrade.MigratorTypeName,
                                    ResourceCode = resourceUpgrade.ResourceCode
                                };

                var template =
                    this.StateData.ReleaseState.States.FirstOrDefault(
                        t => t.Template.Code == resourceUpgrade.TemplateCode);
                if (template == null)
                {
                    error.ErrorMessage = "Migrator template was not found";
                    errors.Add(error);
                    continue;
                }

                error.MigratorTemplateName = template.Template.Name;

                var migrator =
                    template.MigratorsStates.FirstOrDefault(m => m.TypeName == resourceUpgrade.MigratorTypeName);
                if (migrator == null)
                {
                    error.ErrorMessage = "Migrator was not found";
                    errors.Add(error);
                    continue;
                }

                error.MigratorName = migrator.Name;

                var resource = migrator.Resources.FirstOrDefault(r => r.Code == resourceUpgrade.ResourceCode);
                if (resource == null)
                {
                    error.ErrorMessage = "Resource was not found";
                    errors.Add(error);
                    continue;
                }

                error.ResourceName = resource.Name;

                var sideExecutionPlans = plan.SourceExecution;

                MigratorTemplatePlan templatePlan;
                if (!sideExecutionPlans.TryGetValue(resourceUpgrade.TemplateCode, out templatePlan))
                {
                    templatePlan = new MigratorTemplatePlan { Template = template.Template };
                    sideExecutionPlans[resourceUpgrade.TemplateCode] = templatePlan;
                }

                MigratorMigrationsCommand migratorMigrationsCommand;
                if (!templatePlan.MigratorPlans.TryGetValue(
                        resourceUpgrade.MigratorTypeName,
                        out migratorMigrationsCommand))
                {
                    migratorMigrationsCommand =
                        new MigratorMigrationsCommand { TypeName = resourceUpgrade.MigratorTypeName };
                    templatePlan.MigratorPlans[resourceUpgrade.MigratorTypeName] = migratorMigrationsCommand;
                }

                var migrationPoint = migrator.LastDefinedPoint;
                migratorMigrationsCommand.Resources.Add(resource.Code, migrationPoint);
            }

            return plan;
        }

        /// <summary>
        /// Executes the migration for the template
        /// </summary>
        /// <param name="side">
        /// The migration execution side
        /// </param>
        /// <param name="plan">
        /// The migration plan
        /// </param>
        /// <returns>
        /// The generated migration log
        /// </returns>
        private List<MigrationLogRecord> ExecuteMigration(EnMigrationSide side, MigratorTemplatePlan plan)
        {
            var log = new List<MigrationLogRecord>();
            var releaseDir = side == EnMigrationSide.Source
                                 ? Path.Combine(this.StateData.FromReleaseExecutionDir, plan.Template.Code)
                                 : Path.Combine(this.StateData.ToReleaseExecutionDir, plan.Template.Code);

            var releaseId = side == EnMigrationSide.Source
                                ? this.StateData.Migration.FromRelease.Id
                                : this.StateData.Migration.ToReleaseId;

            Context.GetLogger().Info(
                "{Type}: preparing for migration resources of {MigratorTemplateCode} of release {ReleaseId}. {ResourceCount} resources will be migrated",
                this.GetType().Name,
                plan.Template.Code,
                releaseId,
                plan.MigratorPlans.Values.SelectMany(m => m.Resources.Values).Count());


            try
            {
                var collector = new MigrationExecutor { Commands = plan.MigratorPlans.Values.ToList() };

                Context.GetLogger().Info(
                    "{Type}: {MigratorTemplateCode} of release {ReleaseId} migration executor was created",
                    this.GetType().Name,
                    plan.Template.Code,
                    releaseId);

                collector = this.ExecuteMigrator(releaseDir, collector);
                var operations = collector.Result;
                foreach (var logMessage in collector.Logs)
                {
                    Context.GetLogger().Info(
                        "{Type}: migration log - {LogMessage}",
                        this.GetType().Name,
                        logMessage);
                }

                Context.GetLogger().Info(
                    "{Type}: {MigratorTemplateCode} of release {ReleaseId} migration executor was executed",
                    this.GetType().Name,
                    plan.Template.Code,
                    releaseId);

                if (operations != null)
                {
                    foreach (var operation in operations)
                    {
                        operation.ReleaseId = releaseId;
                        operation.MigrationId = this.StateData.Migration.Id;
                        operation.MigratorTemplateCode = plan.Template.Code;
                        operation.MigratorTemplateName = plan.Template.Name;

                        if (operation.Error != null)
                        {
                            operation.Error.ReleaseId = releaseId;
                            operation.Error.MigrationId = this.StateData.Migration.Id;
                            operation.Error.MigratorTemplateCode = plan.Template.Code;
                            operation.Error.MigratorTemplateName = plan.Template.Name;

                            Context.GetLogger()
                                .Error(
                                    "{Type}: Error while executing migration for template {MigratorTemplateCode} of release {ReleaseId}: {ErrorMessage} \n {ErrorStackTrace}",
                                    this.GetType().Name,
                                    plan.Template.Code,
                                    releaseId,
                                    operation.Error.ErrorMessage,
                                    operation.Error.ErrorStackTrace);
                        }
                        else
                        {
                            Context.GetLogger()
                                .Info(
                                    "{Type}: Migration for template {MigratorTemplateCode} of release {ReleaseId} {ResourceCode} was successfully migrated from {SourcePoint} to {DestinationPoint}",
                                    this.GetType().Name,
                                    plan.Template.Code,
                                    releaseId,
                                    operation.ResourceCode,
                                    operation.SourcePoint,
                                    operation.DestinationPoint);
                        }
                    }

                    log.AddRange(operations);
                }

                if (collector.Errors.Any())
                {
                    foreach (var error in collector.Errors)
                    {
                        log.Add(
                            new MigrationError
                                {
                                    ReleaseId = releaseId,
                                    MigrationId = this.StateData.Migration.Id,
                                    MigratorTemplateCode = plan.Template.Code,
                                    MigratorTemplateName = plan.Template.Name,
                                    ErrorMessage = $"Error while executing migration: {error}"
                                });

                        Context.GetLogger()
                            .Error(
                                "{Type}: Error while executing migration for template {MigratorTemplateCode} of release {ReleaseId}: {ErrorMessage}",
                                this.GetType().Name,
                                plan.Template.Code,
                                releaseId,
                                error);
                    }
                }
            }
            catch (Exception exception)
            {
                log.Add(
                    new MigrationError
                        {
                            ReleaseId = releaseId,
                            MigrationId = this.StateData.Migration.Id,
                            MigratorTemplateCode = plan.Template.Code,
                            MigratorTemplateName = plan.Template.Name,
                            ErrorMessage = exception.Message,
                            Exception = exception
                        });
            }

            return log;
        }

        /// <summary>
        /// Executes the migration for the template
        /// </summary>
        /// <param name="plan">
        /// The migration plan
        /// </param>
        /// <returns>
        /// The migration execution log
        /// </returns>
        private List<MigrationLogRecord> ExecuteMigration(MigratorTemplatePlan plan)
        {
            var log = new List<MigrationLogRecord>();
            var releaseDir = Path.Combine(this.StateData.ReleaseExecutionDir, plan.Template.Code);

            try
            {
                var collector = new MigrationExecutor();
                collector.Commands = plan.MigratorPlans.Values.ToList();
                collector = this.ExecuteMigrator(releaseDir, collector);

                var operations = collector.Result;
                if (operations != null)
                {
                    foreach (var operation in operations)
                    {
                        operation.ReleaseId = this.StateData.Release.Id;
                        operation.MigratorTemplateCode = plan.Template.Code;
                        operation.MigratorTemplateName = plan.Template.Name;

                        if (operation.Error != null)
                        {
                            operation.Error.ReleaseId = this.StateData.Release.Id;
                            operation.Error.MigratorTemplateCode = plan.Template.Code;
                            operation.Error.MigratorTemplateName = plan.Template.Name;

                            Context.GetLogger()
                                .Error(
                                    "{Type}: Error while executing migration for template {MigratorTemplateCode} of release {ReleaseId}: {ErrorMessage} \n {ErrorStackTrace}",
                                    this.GetType().Name,
                                    plan.Template.Code,
                                    this.StateData.Release.Id,
                                    operation.Error.ErrorMessage,
                                    operation.Error.ErrorStackTrace);
                        }
                    }

                    log.AddRange(operations);
                }

                if (collector.Errors.Any())
                {
                    foreach (var error in collector.Errors)
                    {
                        log.Add(
                            new MigrationError
                                {
                                    ReleaseId = this.StateData.Release.Id,
                                    MigratorTemplateCode = plan.Template.Code,
                                    MigratorTemplateName = plan.Template.Name,
                                    ErrorMessage = $"Error while executing migration: {error}"
                                });
                        Context.GetLogger()
                            .Error(
                                "{Type}: Error while executing migration for template {MigratorTemplateCode} of release {ReleaseId}: {ErrorMessage}",
                                this.GetType().Name,
                                plan.Template.Code,
                                this.StateData.Release.Id,
                                error);
                    }
                }
            }
            catch (Exception exception)
            {
                log.Add(
                    new MigrationError
                        {
                            ReleaseId = this.StateData.Release.Id,
                            MigratorTemplateCode = plan.Template.Code,
                            MigratorTemplateName = plan.Template.Name,
                            ErrorMessage = exception.Message,
                            Exception = exception
                        });
            }

            return log;
        }

        /// <summary>
        /// Extracts the lib files to execution directory
        /// </summary>
        /// <param name="package">
        /// The package to extract
        /// </param>
        /// <param name="tmpDir">
        /// The temp directory to extract packages
        /// </param>
        /// <param name="executionDir">
        /// The execution directory to load packages
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        private async Task ExtractPackageAsync(IPackageSearchMetadata package, string tmpDir, string executionDir)
        {
            var files = await this.nugetRepository.ExtractPackage(package, this.frameworkName, executionDir, tmpDir);
            if (!files.Any())
            {
                Context.GetLogger()
                    .Warning(
                        "{Type}: Package {PackageId} does not contains compatible files",
                        this.GetType().Name,
                        package.Identity.ToString());
            }
        }

        /// <summary>
        /// Extracts the specified packages for the release migrators
        /// </summary>
        /// <param name="release">
        /// The release
        /// </param>
        /// <param name="executionDirectory">
        /// The execution Directory.
        /// </param>
        /// <param name="migrationId">
        /// The migration Id.
        /// </param>
        /// <param name="forceExtract">
        /// A value indicating whether previous extracted packages should be overwritten
        /// </param>
        /// <returns>
        /// the success of the operation
        /// </returns>
        private async Task<List<MigrationError>> ExtractReleaseMigratorsAsync(
            Release release,
            string executionDirectory,
            int? migrationId,
            bool forceExtract = false)
        {
            var errors = new List<MigrationError>();
            if (!forceExtract && Directory.Exists(executionDirectory))
            {
                return errors;
            }

            Directory.CreateDirectory(executionDirectory);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);
            try
            {
                foreach (var migratorTemplate in release.Configuration.MigratorTemplates)
                {
                    try
                    {
                        await this.ExtractReleaseMigrationTemplateAsync(
                            release,
                            migrationId,
                            migratorTemplate,
                            executionDirectory,
                            forceExtract,
                            tempDir,
                            errors);
                    }
                    catch (Exception exception)
                    {
                        errors.Add(
                            new MigrationError
                                {
                                    ReleaseId = release.Id,
                                    MigrationId = migrationId,
                                    MigratorTemplateCode = migratorTemplate.Code,
                                    MigratorTemplateName = migratorTemplate.Name,
                                    ErrorMessage = $"error on extracting template: {exception.Message}",
                                    Exception = exception
                                });
                    }
                }

                return errors;
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Extracts packages for the <see cref="MigratorTemplate"/>
        /// </summary>
        /// <param name="release">
        /// The release
        /// </param>
        /// <param name="migrationId">
        /// The possible migration id
        /// </param>
        /// <param name="migratorTemplate">
        /// The migrator template to extract
        /// </param>
        /// <param name="executionDirectory">
        /// The execution directory
        /// </param>
        /// <param name="forceExtract">
        /// Whether to overwrite previous extraction
        /// </param>
        /// <param name="tempDir">
        /// The temporary data directory
        /// </param>
        /// <param name="errors">
        /// The list of errors to fill
        /// </param>
        /// <returns>
        /// The async task
        /// </returns>
        private async Task ExtractReleaseMigrationTemplateAsync(Release release, int? migrationId, MigratorTemplate migratorTemplate, string executionDirectory, bool forceExtract, string tempDir, List<MigrationError> errors)
        {
            var migratorExecutionDirectory = Path.Combine(executionDirectory, migratorTemplate.Code);
            if (Directory.Exists(migratorExecutionDirectory))
            {
                if (forceExtract)
                {
                    Directory.Delete(migratorExecutionDirectory, true);
                }
                else
                {
                    return;
                }
            }

            var migratorTempDirectory = Path.Combine(tempDir, migratorTemplate.Code);
            Directory.CreateDirectory(migratorExecutionDirectory);
            Directory.CreateDirectory(migratorTempDirectory);

            List<IPackageSearchMetadata> packages = new List<IPackageSearchMetadata>();
            foreach (var packageDescription in migratorTemplate.PackagesToInstall[this.frameworkName])
            {
                var package = await this.nugetRepository.GetAsync(packageDescription.Id, NuGetVersion.Parse(packageDescription.Version));
                if (package == null)
                {
                    Context.GetLogger()
                        .Error(
                            "{Type} could not find package {PackageName} {PackageVersion} for migrator template {MigratorTemplateCode} of release {ReleaseId}",
                            this.GetType().Name,
                            packageDescription.Id,
                            packageDescription.Version,
                            migratorTemplate.Code,
                            release.Id);
                    errors.Add(
                        new MigrationError
                            {
                                ReleaseId = release.Id,
                                MigrationId = migrationId,
                                MigratorTemplateCode = migratorTemplate.Code,
                                MigratorTemplateName = migratorTemplate.Name,
                                ErrorMessage =
                                    $"could not find package {packageDescription.Id} {packageDescription.Version}"
                            });
                    continue;
                }

                packages.Add(package);
            }

            try
            {
                await this.nugetRepository.CreateServiceAsync(
                    packages,
                    this.frameworkName,
                    migratorExecutionDirectory,
                    "ClusterKit.NodeManager.Migrator.Executor");
            }
            catch (Exception exception)
            {
                Context.GetLogger()
                    .Error(
                        exception,
                        "{Type} Error on creating service for migrator template {MigratorTemplateCode} of release {ReleaseId}",
                        this.GetType().Name,
                        migratorTemplate.Code,
                        release.Id);
                errors.Add(
                    new MigrationError
                        {
                            ReleaseId = release.Id,
                            MigrationId = migrationId,
                            MigratorTemplateCode = migratorTemplate.Code,
                            MigratorTemplateName = migratorTemplate.Name,
                            ErrorMessage =
                                $"error on creating service: {exception.Message}",
                            Exception = exception
                        });
            }

            if (errors.Any())
            {
                Directory.Delete(migratorExecutionDirectory, true);
                return;
            }

            File.WriteAllText(Path.Combine(migratorExecutionDirectory, "config.hocon"), migratorTemplate.Configuration);
        }

        /// <summary>
        /// Calculates the migration direction according to the defined migration points
        /// </summary>
        /// <param name="sourcePoints">The list of migration points in the source release</param>
        /// <param name="destinationPoints">The list of migration points in the destination release</param>
        /// <returns>The migration direction</returns>
        private EnMigrationDirection GetDirection(
            IEnumerable<string> sourcePoints,
            IEnumerable<string> destinationPoints)
        {
            var source = string.Join(", ", sourcePoints.Select(p => $"\"{p.Replace("\\", "\\\\")}\""));
            var destination = string.Join(", ", destinationPoints.Select(p => $"\"{p.Replace("\\", "\\\\")}\""));

            if (source == destination)
            {
                return EnMigrationDirection.Stay;
            }

            if (destination.IndexOf(source, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return EnMigrationDirection.Upgrade;
            }

            if (source.IndexOf(destination, StringComparison.OrdinalIgnoreCase) == 0)
            {
                return EnMigrationDirection.Downgrade;
            }

            return EnMigrationDirection.Undefined;
        }

        /// <summary>
        /// Checks the current migrating resource check
        /// </summary>
        /// <param name="data">
        /// The state data
        /// </param>
        /// <param name="errors">
        /// The list of occurred errors
        /// </param>
        /// <returns>
        /// The check success
        /// </returns>
        private MigrationActorMigrationState GetMigrationState(Data data, out List<MigrationError> errors)
        {
            errors = new List<MigrationError>();
            List<MigrationError> sourceErrors;
            var sourceStates = this.GetReleaseResourcesState(data.Migration.FromRelease, data.FromReleaseExecutionDir, data.Migration.Id, out sourceErrors)
                .ToList();
            List<MigrationError> destinationErrors;
            var destinationStates = this.GetReleaseResourcesState(data.Migration.ToRelease, data.ToReleaseExecutionDir, data.Migration.Id, out destinationErrors)
                .ToList();

            if (sourceErrors.Any() || destinationErrors.Any())
            {
                errors = sourceErrors.Union(destinationErrors).ToList();
                return null;
            }

            var state = this.CreateMigrationState(sourceStates, destinationStates).ToList();
            EnMigrationActorMigrationPosition position;

            var resourcePositions = state.SelectMany(s => s.Migrators)
                .SelectMany(m => m.Resources)
                .Select(r => r.Position)
                .ToList();

            if (!resourcePositions.Any() || resourcePositions.All(p => p == EnResourcePosition.SourceAndDestination))
            {
                position = EnMigrationActorMigrationPosition.NoMigrationNeeded;
            }
            else if (resourcePositions.All(
                p => p == EnResourcePosition.SourceAndDestination || p == EnResourcePosition.Source))
            {
                position = EnMigrationActorMigrationPosition.Source;
            }
            else if (resourcePositions.All(
                p => p == EnResourcePosition.SourceAndDestination || p == EnResourcePosition.Destination))
            {
                position = EnMigrationActorMigrationPosition.Destination;
            }
            else if (resourcePositions.Any(p => p == EnResourcePosition.Undefined))
            {
                position = EnMigrationActorMigrationPosition.Broken;
            }
            else
            {
                position = EnMigrationActorMigrationPosition.PartiallyMigrated;
            }

            var result = new MigrationActorMigrationState { TemplateStates = state, Position = position };
            return result;
        }

        /// <summary>
        /// Executes the <see cref="MigrationCollector"/> from pre-installed service
        /// </summary>
        /// <typeparam name="T">The collector end-type</typeparam>
        /// <param name="installedPath">The service installation path</param>
        /// <param name="instance">The collector instance</param>
        /// <returns>The executed collector</returns>
        private T ExecuteMigrator<T>(string installedPath, T instance) where T : MigrationCollector
        {
#if APPDOMAIN
            const string ExecutableFileName = "ClusterKit.NodeManager.Migrator.Executor.exe";
            const string ExecutableArguments = "";
#elif CORECLR
            const string ExecutableFileName = "dotnet";
            const string ExecutableArguments = "ClusterKit.NodeManager.Migrator.Executor.dll";
#endif
            var process = new Process
                              {
                                  StartInfo =
                                      {
                                          UseShellExecute = false,
                                          WorkingDirectory = installedPath,
                                          FileName = ExecutableFileName,
                                          Arguments = ExecutableArguments,
                                          RedirectStandardOutput = true,
                                          RedirectStandardInput = true,
                                          RedirectStandardError = true
                                      }
                              };

            process.Start();

            string readLine = null;
            while (readLine != ProcessHelper.EOF && !process.HasExited)
            {
                readLine = process.StandardOutput.ReadLine();
            }

            process.StandardInput.Send(instance);
            var output = process.StandardOutput.Receive() as T;
            process.WaitForExit();
            var error = process.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(error))
            {
                Context.GetLogger().Error("{Type}: Migrator exited with error {Error}", this.GetType().Name, error);
            }

            process.Dispose();
            return output;
        }

        /// <summary>
        /// Gets the state of the release
        /// </summary>
        /// <param name="release">
        /// The release to check
        /// </param>
        /// <param name="executionDirectory">
        /// The execution Directory.
        /// </param>
        /// <param name="migrationId">
        /// The current migration id
        /// </param>
        /// <param name="errors">
        /// The list of execution errors
        /// </param>
        /// <returns>
        /// The state of release resources
        /// </returns>
        private List<MigratorTemplateReleaseState> GetReleaseResourcesState(
            Release release,
            string executionDirectory,
            int? migrationId,
            out List<MigrationError> errors)
        {
            var result = new List<MigratorTemplateReleaseState>();
            errors = new List<MigrationError>();

            foreach (var migratorTemplate in release.Configuration.MigratorTemplates.OrderByDescending(
                mt => mt.Priority))
            {
                var state = new MigratorTemplateReleaseState { Code = migratorTemplate.Code, Template = migratorTemplate };
                var releaseDir = Path.Combine(executionDirectory, migratorTemplate.Code);
                try
                {
                    var collector = new ReleaseStateCollector();
                    collector = this.ExecuteMigrator(releaseDir, collector);
                    var collectorErrors = collector.Errors;
                    var migratorReleaseStates = collector.Result;

                    if (collectorErrors.Any())
                    {
                        foreach (var error in collector.Errors)
                        {
                            error.ReleaseId = release.Id;
                            error.MigrationId = migrationId;
                            error.MigratorTemplateCode = migratorTemplate.Code;
                            error.MigratorTemplateName = migratorTemplate.Name;
                            errors.Add(error);

                            Context.GetLogger()
                                .Error(
                                    "{Type}: Error while requesting migration state for migrator "
                                    + "template {MigratorTemplateCode} of release {ReleaseId}: {ErrorMessage}\n {ErrorStackTrace}",
                                    this.GetType().Name,
                                    migratorTemplate.Code,
                                    release.Id,
                                    error.ErrorMessage,
                                    error.ErrorStackTrace);
                        }

                        continue;
                    }

                    state.MigratorsStates = migratorReleaseStates;
                    result.Add(state);
                }
                catch (Exception exception)
                {
                    errors.Add(
                        new MigrationError
                            {
                                ReleaseId = release.Id,
                                MigrationId = migrationId,
                                MigratorTemplateCode = migratorTemplate.Code,
                                MigratorTemplateName = migratorTemplate.Name,
                                ErrorMessage =
                                    $"Error while requesting migration state: {exception.Message}",
                                Exception = exception
                            });
                    Context.GetLogger()
                        .Error(
                            exception,
                            "{Type}: Error while requesting migration state for migrator template {MigratorTemplateCode} of release { ReleaseId}",
                            this.GetType().Name,
                            migratorTemplate.Code,
                            release.Id);
                }
            }

            return result;
        }

        /// <summary>
        /// Loads current state for active migration
        /// </summary>
        /// <param name="migration">The current migration</param>
        /// <param name="forceExtract">A value indicating whether release Nuget packages should be overwritten</param>
        /// <param name="data">Current actor state data</param>
        /// <returns>The next actor state</returns>
        private State<EnState, Data> LoadMigrationState(Migration migration, bool forceExtract, Data data)
        {
            var fromReleaseExecutionDir = data?.FromReleaseExecutionDir
                                          ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            var toReleaseExecutionDir = data?.ToReleaseExecutionDir
                                        ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            var extractionErrors = this
                .ExtractReleaseMigratorsAsync(migration.FromRelease, fromReleaseExecutionDir, migration.Id, forceExtract)
                .GetAwaiter().GetResult()
                .Union(
                    this.ExtractReleaseMigratorsAsync(
                        migration.ToRelease,
                        toReleaseExecutionDir,
                        migration.Id,
                        forceExtract).GetAwaiter().GetResult())
                .ToList();

            if (extractionErrors.Any())
            {
                this.Parent.Tell(new MigrationActorInitializationFailed { Errors = extractionErrors });
                return new State<EnState, Data>(EnState.InitializationFailed, new Data());
            }

            data = new Data
                       {
                           Migration = migration,
                           FromReleaseExecutionDir = fromReleaseExecutionDir,
                           ToReleaseExecutionDir = toReleaseExecutionDir
                       };

            List<MigrationError> errors;
            var state = this.GetMigrationState(data, out errors);

            if (state == null)
            {
                this.Parent.Tell(new MigrationActorInitializationFailed { Errors = errors });
                return new State<EnState, Data>(EnState.InitializationFailed, data);
            }

            data.MigrationState = state;
            this.Parent.Tell(state);
            return new State<EnState, Data>(EnState.Migration, data);
        }

        /// <summary>
        /// Loads current state without active migration
        /// </summary>
        /// <param name="release">
        /// The currently active release
        /// </param>
        /// <param name="forceExtract">
        /// A value indicating whether release Nuget packages should be overwritten
        /// </param>
        /// <param name="data">
        /// Current actor state data
        /// </param>
        /// <returns>
        /// The next actor state
        /// </returns>
        private State<EnState, Data> LoadReleaseState(Release release, bool forceExtract, Data data)
        {
            var releaseExecutionDir = data?.ReleaseExecutionDir
                                      ?? Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            data = new Data { ReleaseExecutionDir = releaseExecutionDir, Release = release };
            var extractionErrors = this.ExtractReleaseMigratorsAsync(release, releaseExecutionDir, null, forceExtract).GetAwaiter().GetResult();
            if (extractionErrors.Any())
            {
                this.Parent.Tell(new MigrationActorInitializationFailed { Errors = extractionErrors });
                return new State<EnState, Data>(EnState.InitializationFailed, data);
            }

            List<MigrationError> errors;
            var releaseStates = this.GetReleaseResourcesState(release, releaseExecutionDir, null, out errors).ToList();
            if (errors.Any())
            {
                this.Parent.Tell(new MigrationActorInitializationFailed { Errors = errors });
                return new State<EnState, Data>(EnState.InitializationFailed, data);
            }

            data.ReleaseState =
                new MigrationActorReleaseState
                    {
                        States =
                            releaseStates
                    };

            this.Parent.Tell(data.ReleaseState);
            return new State<EnState, Data>(EnState.Idle, data);
        }

        /// <summary>
        /// Loads current migration state
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <param name="forceExtract">
        /// A value indicating whether previous extracted packages should be overwritten
        /// </param>
        /// <returns>
        /// The current state
        /// </returns>
        private State<EnState, Data> LoadState(Data data = null, bool forceExtract = false)
        {
            if (data == null)
            {
                data = this.StateData;
            }

            using (var ds = this.contextFactory.CreateContext<ConfigurationContext>(
                this.databaseProviderName,
                this.connectionString,
                this.databaseName))
            {
                var currentMigration = ds.Migrations.Include(nameof(Migration.FromRelease))
                    .Include(nameof(Migration.ToRelease)).FirstOrDefault(m => m.IsActive);

                if (currentMigration != null)
                {
                    return this.LoadMigrationState(currentMigration, forceExtract, data);
                }

                var release = ds.Releases.First(r => r.State == EnReleaseState.Active);
                return this.LoadReleaseState(release, forceExtract, data);
            }
        }

        /// <summary>
        /// Handles the <see cref="List{T}"/> of <see cref="ResourceUpgrade"/> message in <see cref="EnState.Migration"/> state
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The next state</returns>
        private State<EnState, Data> StateIdleHandleResourceUpgrade(List<ResourceUpgrade> request)
        {
            List<MigrationError> errors;
            var plan = this.CreateReleasePlan(request, out errors);

            if (errors.Count != 0 || this.StateData.ReleaseState == null)
            {
                this.Sender.Tell(new RequestDeclined { Errors = errors });
                return this.Stay();
            }

            this.Sender.Tell(new RequestAcknowledged());
            this.Parent.Tell(new ProcessingTheRequest());

            var migrationLog = plan.SourceExecution.Values.Aggregate(
                (IEnumerable<MigrationLogRecord>)new List<MigrationLogRecord>(),
                (list, templatePlan) => list.Union(this.ExecuteMigration(templatePlan))).ToList();
            this.Parent.Tell(migrationLog);
            return this.LoadState();
        }

        /// <summary>
        /// Handles the <see cref="List{T}"/> of <see cref="ResourceUpgrade"/> message in <see cref="EnState.Migration"/> state
        /// </summary>
        /// <param name="request">The request</param>
        /// <returns>The next state</returns>
        private State<EnState, Data> StateMigrationHandleResourceUpgrade(List<ResourceUpgrade> request)
        {
            List<MigrationError> errors;
            var plan = this.CreateMigrationPlan(request, out errors);

            if (errors.Count != 0 || this.StateData.MigrationState == null)
            {
                Context.GetLogger()
                    .Error(
                        "{Type}: Failed to fulfill migration request. Could not create migration plan.",
                        this.GetType().Name);
                this.Parent.Tell(errors.Cast<MigrationLogRecord>().ToList());
                return this.Stay();
            }

            var sourceLog = plan.SourceExecution.Values.Aggregate(
                (IEnumerable<MigrationLogRecord>)new List<MigrationLogRecord>(),
                (list, templatePlan) => list.Union(this.ExecuteMigration(EnMigrationSide.Source, templatePlan)));

            var destinationLog = plan.DestinationExecution.Values.Aggregate(
                (IEnumerable<MigrationLogRecord>)new List<MigrationLogRecord>(),
                (list, templatePlan) => list.Union(this.ExecuteMigration(EnMigrationSide.Destination, templatePlan)));

            this.Parent.Tell(sourceLog.Union(destinationLog).ToList());
            return this.LoadState();
        }

        /// <summary>
        /// The current data
        /// </summary>
        public class Data
        {
            /// <summary>
            /// Gets or sets the directory to extract migrator code for the initial release configuration
            /// </summary>
            public string FromReleaseExecutionDir { get; set; }

            /// <summary>
            /// Gets or sets the current migration
            /// </summary>
            public Migration Migration { get; set; }

            /// <summary>
            /// Gets or sets the migration state
            /// </summary>
            public MigrationActorMigrationState MigrationState { get; set; }

            /// <summary>
            /// Gets or sets the current release
            /// </summary>
            public Release Release { get; set; }

            /// <summary>
            /// Gets or sets the release execution directory 
            /// </summary>
            public string ReleaseExecutionDir { get; set; }

            /// <summary>
            /// Gets or sets the current release
            /// </summary>
            public MigrationActorReleaseState ReleaseState { get; set; }

            /// <summary>
            /// Gets or sets the directory to extract migrator code for the destination release configuration
            /// </summary>
            public string ToReleaseExecutionDir { get; set; }
        }

        /// <summary>
        /// The resource migration plan to resolve migration request
        /// </summary>
        private class MigrationPlan
        {
            /// <summary>
            /// Gets the list of migrations that should be performed with the destination release code
            /// </summary>
            public Dictionary<string, MigratorTemplatePlan> DestinationExecution { get; } =
                new Dictionary<string, MigratorTemplatePlan>();

            /// <summary>
            /// Gets the list of migrations that should be performed with the source release code
            /// </summary>
            public Dictionary<string, MigratorTemplatePlan> SourceExecution { get; } =
                new Dictionary<string, MigratorTemplatePlan>();
        }

        /// <summary>
        /// The migration plan to execute with specific migrator template
        /// </summary>
        private class MigratorTemplatePlan
        {
            /// <summary>
            /// Gets the list of command to execute with migrators
            /// </summary>
            public Dictionary<string, MigratorMigrationsCommand> MigratorPlans { get; } =
                new Dictionary<string, MigratorMigrationsCommand>();

            /// <summary>
            /// Gets or sets the migrator template
            /// </summary>
            public MigratorTemplate Template { get; set; }
        }
    }
}