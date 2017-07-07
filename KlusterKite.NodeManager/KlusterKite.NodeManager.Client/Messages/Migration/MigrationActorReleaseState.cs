﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigrationActorReleaseState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The <see cref="MigrationActor" /> state with no active migration
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages.Migration
{
    using System.Collections.Generic;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.MigrationStates;

    /// <summary>
    /// The MigrationActor state with no active migration
    /// </summary>
    [ApiDescription("The MigrationActor state with no active migration", Name = "MigrationActorReleaseState")] 
    public class MigrationActorReleaseState
    {
        /// <summary>
        /// Gets or sets the list of template states
        /// </summary>
        [DeclareField("the list of template states")]
        public List<MigratorTemplateReleaseState> States { get; set; }
    }
}
