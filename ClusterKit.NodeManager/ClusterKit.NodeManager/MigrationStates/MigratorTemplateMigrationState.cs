﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorTemplateMigrationState.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The migrator template state according to the migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.MigrationStates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.RemoteDomain;

    /// <summary>
    /// The migrator template state according to the migration
    /// </summary>
    public class MigratorTemplateMigrationState
    {
        /// <summary>
        /// Gets or sets the template code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the migrator template from source release
        /// </summary>
        public MigratorTemplate SourceTemplate { get; set; }

        /// <summary>
        /// Gets or sets the migrator template from destination release
        /// </summary>
        public MigratorTemplate DestinationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the current migrator position
        /// </summary>
        public EnMigratorPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the list of states of migrators
        /// </summary>
        public List<MigratorMigrationState> Migrators { get; set; }

        /// <summary>
        /// Creates <see cref="MigratorTemplateMigrationState"/> from <see cref="MigratorTemplateReleaseState"/>
        /// </summary>
        /// <param name="state">The state of template</param>
        /// <param name="position">The template position</param>
        /// <returns>The new <see cref="MigratorTemplateMigrationState"/></returns>
        public static MigratorTemplateMigrationState CreateFrom(
            MigratorTemplateReleaseState state,
            EnMigratorPosition position)
        {
            if (position != EnMigratorPosition.New && position != EnMigratorPosition.Obsolete)
            {
                throw new ArgumentException(@"Can be called only for new or obsolete templates", nameof(position));
            }

            return new MigratorTemplateMigrationState
                       {
                           Code = state.Template.Code,
                           SourceTemplate =
                               position == EnMigratorPosition.Obsolete
                                   ? state.Template
                                   : null,
                           DestinationTemplate =
                               position == EnMigratorPosition.New
                                   ? state.Template
                                   : null,
                           Position = position,
                           Migrators =
                               state.MigratorsStates
                                   .Select(
                                       m => MigratorMigrationState.CreateFrom(
                                           m,
                                           position))
                                   .ToList()
                       };
        }
    }
}
