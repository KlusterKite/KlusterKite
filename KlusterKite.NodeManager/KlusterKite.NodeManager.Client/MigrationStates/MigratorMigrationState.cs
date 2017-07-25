// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorMigrationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The migrator state according to the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The migrator state according to the migration
    /// </summary>
    [ApiDescription("The migrator state according to the migration", Name = "MigratorMigrationState")]
    public class MigratorMigrationState
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        [DeclareField("the migrator type name", IsKey = true)]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IMigrator.Name"/>
        /// </summary>
        [DeclareField("the migrator name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the current migrator position
        /// </summary>
        [DeclareField("the current migrator position")]
        public EnMigratorPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the list of resources states
        /// </summary>
        [DeclareField("the list of resources states")]
        public List<ResourceMigrationState> Resources { get; set; }

        /// <summary>
        /// Gets or sets the migration direction
        /// </summary>
        [DeclareField("the migration direction")]
        public EnMigrationDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the migrator execution priority
        /// </summary>
        [DeclareField("the migrator execution priority")]
        [UsedImplicitly]
        public decimal Priority { get; set; }

        /// <summary>
        /// Gets or sets the resource dependency type
        /// </summary>
        [DeclareField("the resource dependency type")]
        public EnResourceDependencyType DependencyType { get; set; }

        /// <summary>
        /// Creates <see cref="MigratorMigrationState"/> from <see cref="MigratorConfigurationState"/>
        /// </summary>
        /// <param name="templateCode">The migrator template code</param>
        /// <param name="state">The state of template</param>
        /// <param name="position">The migration position</param>
        /// <returns>The new <see cref="MigratorTemplateMigrationState"/></returns>
        public static MigratorMigrationState CreateFrom(string templateCode, MigratorConfigurationState state, EnMigratorPosition position)
        {
            if (position != EnMigratorPosition.New && position != EnMigratorPosition.Obsolete)
            {
                throw new ArgumentException(@"Can be called only for new or obsolete templates", nameof(position));
            }

            var resources = state.Resources.Select(r => ResourceMigrationState.CreateFrom(templateCode, state, r, position)).ToList();

            return new MigratorMigrationState
                       {
                           Name = state.Name,
                           TypeName = state.TypeName,
                           Position = position,
                           Direction = EnMigrationDirection.Stay,
                           DependencyType = state.DependencyType,
                           Priority = state.Priority,
                           Resources = resources
                       };
        }
    }
}
