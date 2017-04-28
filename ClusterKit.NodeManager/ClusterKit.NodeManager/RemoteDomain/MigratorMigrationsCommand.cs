// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MigratorMigrationsCommand.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of migrator action to migrate resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.RemoteDomain
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The description of migrator action to migrate resources
    /// </summary>
    [Serializable]
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