// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceUpgrade.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to migrate specific resource
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages.Migration
{
    using ClusterKit.API.Attributes;
    using ClusterKit.NodeManager.Client.MigrationStates;
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.NodeManager.Migrator;

    using JetBrains.Annotations;

    /// <summary>
    /// The request to migrate specific resource
    /// </summary>
    [ApiDescription("The command to update resource", Name = "ResourceUpgrade")]
    public class ResourceUpgrade
    {
        /// <summary>
        /// Gets or sets the request id
        /// </summary>
        /// <remarks>
        /// Made to fulfill GraphQL id presence requirement
        /// </remarks>
        [UsedImplicitly]
        [DeclareField("the request id", IsKey = true, Access = EnAccessFlag.Queryable)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the code of <see cref="MigratorTemplate.Code"/>
        /// </summary>
        [DeclareField("The code of migrator template")]
        public string TemplateCode { get; set; }

        /// <summary>
        /// Gets or sets the type name of <see cref="IMigrator"/>
        /// </summary>
        [DeclareField("The type name of the migrator")]
        public string MigratorTypeName { get; set; }

        /// <summary>
        /// Gets or sets the migrating resource code from <see cref="ResourceId.Code"/>
        /// </summary>
        [DeclareField("The resource code")]
        public string ResourceCode { get; set; }

        /// <summary>
        /// Gets or sets a target migration point
        /// </summary>
        [DeclareField("The update target")]
        public EnMigrationSide Target { get; set; }
    }
}
