// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorMigrationsCommand.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The description of migrator action to migrate resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.RemoteDomain
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The description of migrator action to migrate resources within same migrator
    /// </summary>
    public class MigratorMigrationsCommand 
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the list of resources to migrate
        /// </summary>
        [UsedImplicitly]
        public List<ResourceMigrationCommand> Resources { get; set; } = new List<ResourceMigrationCommand>();
    }
}