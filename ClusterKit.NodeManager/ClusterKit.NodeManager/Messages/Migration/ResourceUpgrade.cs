// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceUpgrade.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to migrate specific resource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages.Migration
{
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.MigrationStates;
    using ClusterKit.NodeManager.Migrator;

    /// <summary>
    /// The request to migrate specific resource
    /// </summary>
    public class ResourceUpgrade
    {
        /// <summary>
        /// Gets or sets the code of <see cref="MigratorTemplate.Code"/>
        /// </summary>
        public string TemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the type name of <see cref="IMigrator"/>
        /// </summary>
        public string MigratorTypeName { get; set; }

        /// <summary>
        /// Gets or sets the migrating resource code from <see cref="ResourceId.Code"/>
        /// </summary>
        public string ResourceCode { get; set; }

        /// <summary>
        /// Gets or sets a target migration point
        /// </summary>
        public EnMigrationSide Target { get; set; }
    }
}
