// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationExecutor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Executes migrations for the specified resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;

    using ClusterKit.NodeManager.Client.ORM;

    using JetBrains.Annotations;

    /// <summary>
    /// Executes migrations for the specified resources
    /// </summary>
    public class MigrationExecutor : MigrationCollector<List<MigrationOperation>>
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
        protected override List<MigrationOperation> GetTypedResult(IComponentContext context)
        {
            this.Logs.Add("Started");
            var result = new List<MigrationOperation>();
            var migrators = this.GetMigrators(context).ToList();
            foreach (var command in this.Commands)
            {
                this.Logs.Add($"Executing migrator {command.TypeName}");
                var migrator = migrators.FirstOrDefault(m => m.GetType().FullName == command.TypeName);
                if (migrator == null)
                {
                    this.Errors.Add(
                        new MigrationError
                            {
                                MigratorTypeName = command.TypeName,
                                ErrorMessage = "Migrator is not defined"
                            });
                    this.Logs.Add($"Migrator {command.TypeName} was not found");
                    continue;
                }
                
                this.Logs.Add($"Migrator {command.TypeName} found");

                var resources = migrator.GetMigratableResources().ToList();
                var points = migrator.GetAllPoints().ToList();

                foreach (var pair in command.Resources)
                {
                    var operation = new MigrationOperation
                                        {
                                            MigratorTypeName = command.TypeName,
                                            MigratorName = migrator.Name,
                                            ResourceCode = pair.Key,
                                            Started = DateTimeOffset.Now
                                        };
                    result.Add(operation);

                    var resource = resources.FirstOrDefault(r => r.Code == pair.Key);
                    if (resource == null)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Error =
                            new MigrationError
                                {
                                    MigratorTypeName = command.TypeName,
                                    MigratorName = migrator.Name,
                                    ResourceCode = pair.Key,
                                    ErrorMessage = "Resource is not defined in the migrator"
                                };
                        this.Logs.Add($"Resource {pair.Key} is not defined in the migrator");
                        continue;
                    }

                    try
                    {
                        operation.SourcePoint = migrator.GetCurrentPoint(resource);
                        operation.DestinationPoint = pair.Value;
                    }
                    catch (Exception exception)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Error =
                            new MigrationError
                                {
                                    MigratorTypeName = command.TypeName,
                                    MigratorName = migrator.Name,
                                    ResourceCode = pair.Key,
                                    ErrorMessage =
                                        $"Exception while checking resource current point: {exception.Message}",
                                    Exception = exception
                                };
                        this.Logs.Add($"Exception while checking resource {pair.Key} current point: {exception.Message}");
                        continue;
                    }

                    if (!points.Contains(pair.Value))
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Error =
                            new MigrationError
                                {
                                    MigratorTypeName = command.TypeName,
                                    MigratorName = migrator.Name,
                                    ResourceCode = pair.Key,
                                    ErrorMessage = "Resource cannot migrate to point"
                                };
                        this.Logs.Add($"Resource {pair.Key} cannot migrate to point");
                        continue;
                    }

                    try
                    {
                        this.Logs.AddRange(migrator.Migrate(resource, pair.Value));
                    }
                    catch (Exception exception)
                    {
                        operation.Finished = DateTimeOffset.Now;
                        operation.Error =
                            new MigrationError
                                {
                                    MigratorTypeName = command.TypeName,
                                    MigratorName = migrator.Name,
                                    ResourceCode = pair.Key,
                                    ErrorMessage =
                                        $"Exception while migrating resource: {exception.Message}",
                                    Exception = exception
                                };
                        this.Logs.Add($"Exception while migrating resource {pair.Key}: {exception.Message}");
                    }

                    operation.Finished = DateTimeOffset.Now;
                    this.Logs.Add($"Resource {pair.Key} was migrated successfully");
                }
            }

            return result;
        }
    }
}
