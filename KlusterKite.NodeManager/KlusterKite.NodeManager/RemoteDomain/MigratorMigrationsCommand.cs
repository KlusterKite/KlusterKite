﻿// --------------------------------------------------------------------------------------------------------------------
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

    /// <summary>
    /// The description of migrator action to migrate resources
    /// </summary>
    public class MigratorMigrationsCommand 
    {
        /// <summary>
        /// Gets or sets the migrator type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the list of resources to migrate (pair of resource code and desired migration point)
        /// </summary>
        public Dictionary<string, string> Resources { get; set; } = new Dictionary<string, string>();
    }
}