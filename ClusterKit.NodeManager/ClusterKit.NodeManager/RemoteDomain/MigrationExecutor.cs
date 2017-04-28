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

    using ClusterKit.NodeManager.Client.ORM;

    /// <summary>
    /// Executes migrations for the specified resources
    /// </summary>
    public class MigrationExecutor : MigrationCollector<List<MigrationOperation>>
    {
        /// <summary>
        /// Gets or sets the list of migration commands
        /// </summary>
        public List<MigratorMigrationsCommand> Commands { get; set; }

        /// <inheritdoc />
        protected override List<MigrationOperation> GetResult()
        {
            var result = new List<MigrationOperation>();
            var migrators = this.GetMigrators().ToList();
            foreach (var command in this.Commands)
            {
                var migrator = migrators.FirstOrDefault(m => m.GetType().FullName == command.TypeName);
                if (migrator == null)
                {
                    this.Errors.Add(
                        new MigrationError
                            {
                                MigratorTypeName = command.TypeName,
                                ErrorMessage = "Migrator is not defined"
                            });
                    continue;
                }

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
                        continue;
                    }

                    try
                    {
                        migrator.Migrate(resource, pair.Value);
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
                    }

                    operation.Finished = DateTimeOffset.Now;
                }
            }

            return result;
        }
    }
}
