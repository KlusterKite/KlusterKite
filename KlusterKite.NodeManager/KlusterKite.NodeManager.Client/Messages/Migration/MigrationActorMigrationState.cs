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

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.MigrationStates;

    /// <summary>
    ///  The MigrationActor state with active migration
    /// </summary>
    [ApiDescription("The MigrationActor state with active migration", Name = "MigrationActorMigrationState")]
    public class MigrationActorMigrationState
    {
        /// <summary>
        /// Gets or sets the current migration position
        /// </summary>
        [DeclareField("the current migration position")]
        public EnMigrationActorMigrationPosition Position { get; set; }

        /// <summary>
        /// Gets or sets the list of migration states
        /// </summary>
        [DeclareField("the list of migration states")]
        public List<MigratorTemplateMigrationState> TemplateStates { get; set; }
    }
}
