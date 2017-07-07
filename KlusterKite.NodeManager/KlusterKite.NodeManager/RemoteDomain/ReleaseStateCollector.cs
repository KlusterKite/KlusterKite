// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseStateCollector.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Gathers the state of resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Autofac;

    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// Gathers the state of resources
    /// </summary>
    public class ReleaseStateCollector : MigrationCollector<List<MigratorReleaseState>>
    {
        /// <inheritdoc />
        protected override List<MigratorReleaseState> GetTypedResult(IComponentContext context)
        {
            var result = new List<MigratorReleaseState>();
            foreach (var migrator in this.GetMigrators(context))
            {
                List<ResourceId> resources;
                try
                {
                    resources = migrator.GetMigratableResources().ToList();
                }
                catch (Exception exception)
                {
                    this.Errors.Add(
                        new MigrationLogRecord
                            {
                                Type = EnMigrationLogRecordType.Error,
                                MigratorTypeName = migrator.GetType().FullName,
                                MigratorName = migrator.Name,
                                ErrorMessage =
                                    $"Error on requesting migratable resources: {exception.Message}",
                                Exception = exception
                            });
                    continue;
                }

                List<string> migrationPoints;
                try
                {
                    migrationPoints = migrator.GetAllPoints().ToList();
                }
                catch (Exception exception)
                {
                    this.Errors.Add(
                        new MigrationLogRecord
                            {
                                Type = EnMigrationLogRecordType.Error,
                                MigratorTypeName = migrator.GetType().FullName,
                                MigratorName = migrator.Name,
                                ErrorMessage =
                                    $"Error on requesting available migration points: {exception.Message}",
                                Exception = exception
                            });
                    continue;
                }

                var states = new List<ResourceReleaseState>();
                foreach (var resource in resources)
                {
                    try
                    {
                        states.Add(
                            new ResourceReleaseState
                                {
                                    Name = resource.Name,
                                    Code = resource.Code,
                                    CurrentPoint = migrator.GetCurrentPoint(resource)
                                });
                    }
                    catch (Exception exception)
                    {
                        this.Errors.Add(
                            new MigrationLogRecord
                                {
                                    Type = EnMigrationLogRecordType.Error,
                                    MigratorTypeName = migrator.GetType().FullName,
                                    MigratorName = migrator.Name,
                                    ResourceCode = resource.Code,
                                    ResourceName = resource.Name,
                                    ErrorMessage =
                                        $"Error on requesting resource current point: {exception.Message}",
                                    Exception = exception
                                });
                    }
                }

                result.Add(
                    new MigratorReleaseState
                        {
                            TypeName = migrator.GetType().FullName,
                            Name = migrator.Name,
                            MigrationPoints = migrationPoints,
                            LastDefinedPoint = migrator.LatestPoint,
                            Resources = states
                        });
            }

            return result;
        }
    }
}
