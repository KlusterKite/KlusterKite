// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorMigrationState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The migrator state according to the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.MigrationStates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Migrator;

    /// <summary>
    /// The migrator state according to the migration
    /// </summary>
    public class MigratorMigrationState
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMigrator.Name"/>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current migrator position
        /// </summary>
        public EnMigratorPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the list of resources states
        /// </summary>
        public List<ResourceMigrationState> Resources { get; set; }

        /// <summary>
        /// Gets or sets the migration direction
        /// </summary>
        public EnMigrationDirection Direction { get; set; }

        /// <summary>
        /// Creates <see cref="MigratorMigrationState"/> from <see cref="MigratorReleaseState"/>
        /// </summary>
        /// <param name="state">The state of template</param>
        /// <param name="position">The migration position</param>
        /// <returns>The new <see cref="MigratorTemplateMigrationState"/></returns>
        public static MigratorMigrationState CreateFrom(MigratorReleaseState state, EnMigratorPosition position)
        {
            if (position != EnMigratorPosition.New && position != EnMigratorPosition.Obsolete)
            {
                throw new ArgumentException(@"Can be called only for new or obsolete templates", nameof(position));
            }

            var direction = position == EnMigratorPosition.New
                                ? EnMigrationDirection.Upgrade
                                : EnMigrationDirection.Stay;

            return new MigratorMigrationState
                       {
                           Name = state.Name,
                           TypeName = state.TypeName,
                           Position = position,
                           Direction = direction,
                           Resources =
                               state.Resources
                                   .Select(
                                       r => ResourceMigrationState.CreateFrom(
                                           state,
                                           r,
                                           position))
                                   .ToList()
                       };
        }
    }
}
