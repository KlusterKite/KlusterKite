// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorMigrationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The notification of current migration check complete
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;
    using System.Linq;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.MigrationStates;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    ///  The MigrationActor state with active migration
    /// </summary>
    [ApiDescription("The MigrationActor state with active migration", Name = "MigrationActorMigrationState")]
    public class MigrationActorMigrationState
    {
        /// <summary>
        /// Gets or sets the list of migration states
        /// </summary>
        [DeclareField("the list of migration states")]
        public List<MigratorTemplateMigrationState> TemplateStates { get; set; }

        /// <summary>
        /// Gets or sets the list of resources that can be migrated on current stage
        /// </summary>
        [DeclareField("the list of resources that can be migrated on current stage")]
        public List<ResourceMigrationState> MigratableResources { get; set; }

        /// <summary>
        /// Gets the list of resources that are migratable at <see cref="EnMigrationSteps.PreNodesResourcesUpdating"/> stage
        /// </summary>
        /// <returns>The list of resources</returns>
        public IEnumerable<ResourceMigrationState> GetPreNodesMigratableResources()
        {
            var enumerable = this.TemplateStates.SelectMany(t => t.Migrators)
                .SelectMany(m => m.Resources.Select(r => new { m, r }))
                .Where(
                    p => ((p.r.Position == EnResourcePosition.NotCreated
                           && p.m.DependencyType == EnResourceDependencyType.CodeDependsOnResource)
                          || (p.r.Position != EnResourcePosition.NotCreated
                              && ((p.m.Direction == EnMigrationDirection.Upgrade && p.m.DependencyType
                                   == EnResourceDependencyType.CodeDependsOnResource)
                                  || (p.m.Direction == EnMigrationDirection.Downgrade && p.m.DependencyType
                                      == EnResourceDependencyType.ResourceDependsOnCode))))
                         && p.r.Position != EnResourcePosition.SourceAndDestination
                         && p.r.Position != EnResourcePosition.OutOfScope
                         && p.r.Position != EnResourcePosition.Obsolete).ToList();

            return enumerable.Where(r => r.m.Direction == EnMigrationDirection.Downgrade).OrderBy(r => r.m.Priority)
                .Union(enumerable.Where(r => r.m.Direction != EnMigrationDirection.Downgrade).OrderByDescending(r => r.m.Priority))
                .Select(p => p.r);
        }

        /// <summary>
        /// Gets the list of resources that are migratable at <see cref="EnMigrationSteps.PostNodesResourcesUpdating"/> stage
        /// </summary>
        /// <returns>The list of resources</returns>
        public IEnumerable<ResourceMigrationState> GetPostNodesMigratableResources()
        {
            var enumerable = this.TemplateStates.SelectMany(t => t.Migrators)
                .SelectMany(m => m.Resources.Select(r => new { m, r }))
                .Where(
                    p => ((p.r.Position == EnResourcePosition.NotCreated
                           && p.m.DependencyType == EnResourceDependencyType.ResourceDependsOnCode)
                          || (p.r.Position != EnResourcePosition.NotCreated
                              && ((p.m.Direction == EnMigrationDirection.Upgrade && p.m.DependencyType
                                   == EnResourceDependencyType.ResourceDependsOnCode)
                                  || (p.m.Direction == EnMigrationDirection.Downgrade && p.m.DependencyType
                                      == EnResourceDependencyType.CodeDependsOnResource))))
                         && p.r.Position != EnResourcePosition.SourceAndDestination
                         && p.r.Position != EnResourcePosition.OutOfScope
                         && p.r.Position != EnResourcePosition.Obsolete).ToList();

            return enumerable.Where(r => r.m.Direction == EnMigrationDirection.Downgrade).OrderBy(r => r.m.Priority)
                .Union(enumerable.Where(r => r.m.Direction != EnMigrationDirection.Downgrade).OrderByDescending(r => r.m.Priority))
                .Select(p => p.r);
        }

        /// <summary>
        /// Gets the list of migration steps
        /// </summary>
        /// <returns>
        /// the list of migration steps
        /// </returns>
        public IEnumerable<EnMigrationSteps> GetMigrationSteps()
        {
            yield return EnMigrationSteps.Start;

            if (this.GetPreNodesMigratableResources().Any())
            {
                yield return EnMigrationSteps.PreNodesResourcesUpdating;
                yield return EnMigrationSteps.PreNodeResourcesUpdated;
            }

            yield return EnMigrationSteps.NodesUpdating;

            if (this.GetPostNodesMigratableResources().Any())
            {
                yield return EnMigrationSteps.NodesUpdated;
                yield return EnMigrationSteps.PostNodesResourcesUpdating;
            }

            yield return EnMigrationSteps.Finish;
        }
    }
}
