// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorTemplateConfigurationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The state of resources of the migrator template
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using System.Collections.Generic;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.ORM;

    /// <summary>
    /// The state of resources of the migrator template
    /// </summary>
    [ApiDescription("The state of resources of the migrator template", Name = "MigratorTemplateConfigurationState")]
    public class MigratorTemplateConfigurationState
    {
        /// <summary>
        /// Gets or sets the template code
        /// </summary>
        [DeclareField("the template code", IsKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the migrator template
        /// </summary>
        [DeclareField("the migrator template")]
        public MigratorTemplate Template { get; set; }

        /// <summary>
        /// Gets or sets the list of migrator states
        /// </summary>
        [DeclareField("the migrator states")]
        public List<MigratorConfigurationState> MigratorsStates { get; set; }
    }
}
