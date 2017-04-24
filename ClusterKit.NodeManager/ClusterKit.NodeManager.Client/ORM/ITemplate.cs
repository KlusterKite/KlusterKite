// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ITemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The union for <see cref="MigratorTemplate" /> and <see cref="NodeTemplate" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System.Collections.Generic;

    using ClusterKit.NodeManager.Launcher.Messages;

    /// <summary>
    /// The union for <see cref="MigratorTemplate"/> and <see cref="NodeTemplate"/>
    /// </summary>
    public interface ITemplate
    {
        /// <summary>
        /// Gets or sets the program readable migrator template name
        /// </summary>
        string Code { get; set; }

        /// <summary>
        /// Gets or sets akka configuration template for migrator. This should contain all needed connection strings and so on
        /// </summary>
        string Configuration { get; set; }
        
        /// <summary>
        /// Gets or sets the list of package requirements
        /// </summary>
        List<NodeTemplate.PackageRequirement> PackageRequirements { get; set; }

        /// <summary>
        /// Gets or sets the list of packages to install for current template
        /// </summary>
        Dictionary<string, List<PackageDescription>> PackagesToInstall { get; set; }
    }
}