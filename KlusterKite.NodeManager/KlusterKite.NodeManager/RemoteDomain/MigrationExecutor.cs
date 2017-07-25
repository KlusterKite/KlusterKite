// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationExecutor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Executes migrations for the specified resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.NodeManager.Client.ORM;

    /// <summary>
    /// Executes migrations for the specified resources
    /// </summary>
    public class MigrationExecutor : MigrationCollector<List<MigrationLogRecord>>
    {
        /// <summary>
        /// Gets or sets the list of migration commands
        /// </summary>
        public List<MigratorMigrationsCommand> Commands { get; set; }

        /// <summary>
        /// Gets or sets the additional execution logs
        /// </summary>
        [UsedImplicitly]
        public List<string> Logs { get; set; } = new List<string>();

        /// <inheritdoc />
        protected override List<MigrationLogRecord> GetTypedResult(IComponentContext context)
        {
            this.Logs.Add("Started");
            var result = new List<MigrationLogRecord>();
            var migrators = this.GetMigrators(context).ToList();
            foreach (var command in this.Commands)
            {
                this.Logs.Add($"Executing migrator {command.TypeName}");
                var migrator = migrators.FirstOrDefault(m => m.GetType().FullName == command.TypeName);
                if (migrator == null)
                {
                    this.Errors.Add(
                        new MigrationLogRecord
                            {
                                Type = EnMigrationLogRecordType.Error,
                                MigratorTypeName = command.TypeName,
                                Message = "Migrator is not defined"
                            });
                    this.Logs.Add($"Migrator {command.TypeName} was not found");
                    continue;
                }
                
                this.Logs.Add($"Migrator {command.TypeName} found");

                var resources = migrator.GetMigratableResources().ToList();
                var points = migrator.GetAllPoints().ToList();

                foreach (var resourceCommand in command.Resources)
                {
                    var operation = new MigrationLogRecord
                                        {
                                            Type = EnMigrationLogRecordType.Operation,
                                            MigratorTypeName = command.TypeName,
                                            MigratorName = migrator.Name,
                                            ResourceCode = resourceCommand.ResourceCode,
                                            Started = DateTimeOffset.Now
                                        };
                    result.Add(operation);

                    var resource = resources.FirstOrDefault(r => r.Code == resourceCommand.ResourceCode);
                    if (resource == null)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Type = EnMigrationLogRecordType.OperationError;
                        operation.Message = "Resource is not defined in the migrator";
                        continue;
                    }

                    operation.ResourceName = resource.Name;

                    try
                    {
                        operation.SourcePoint = migrator.GetCurrentPoint(resource);
                        operation.DestinationPoint = resourceCommand.MigrationPoint;
                    }
                    catch (Exception exception)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Type = EnMigrationLogRecordType.OperationError;
                        operation.Message =
                            $"Exception while checking resource current point: {exception.Message}";
                        operation.Exception = exception;
                        this.Logs.Add($"Exception while checking resource {resourceCommand.ResourceCode} current point: {exception.Message}");
                        continue;
                    }

                    if (!points.Contains(resourceCommand.MigrationPoint))
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Type = EnMigrationLogRecordType.OperationError;
                        operation.Message = "Resource cannot migrate to point";
                        this.Logs.Add($"Resource {resourceCommand.ResourceCode} cannot migrate to point");
                        continue;
                    }

                    try
                    {
                        this.Logs.AddRange(migrator.Migrate(resource, resourceCommand.MigrationPoint));
                    }
                    catch (Exception exception)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Type = EnMigrationLogRecordType.OperationError;
                        operation.Message =
                            $"Exception while migrating resource: {exception.Message}";
                        operation.Exception = exception;
                        this.Logs.Add($"Exception while migrating resource {resourceCommand.ResourceCode}: {exception.Message}");
                    }

                    operation.Finished = DateTimeOffset.Now;
                    this.Logs.Add($"Resource {resourceCommand.ResourceCode} was migrated successfully");
                }
            }

            return result;
        }
    }
}
