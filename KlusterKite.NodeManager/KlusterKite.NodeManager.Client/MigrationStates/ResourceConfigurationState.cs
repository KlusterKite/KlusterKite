// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceConfigurationState.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The resource description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.MigrationStates
{
    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.ORM;
    using KlusterKite.NodeManager.Migrator;

    /// <summary>
    /// The resource description
    /// </summary>
    [ApiDescription("The resource description", Name = "ResourceConfigurationState")]
    public class ResourceConfigurationState
    {
        /// <summary>
        /// The resource unique key
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the resource unique key", IsKey = true)]
        public string Key => $"{this.TemplateCode}:{this.MigratorTypeName}:{this.Code}";

        /// <summary>
        /// Gets or sets the human readable resource name
        /// </summary>
        [DeclareField("the human readable resource name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the resource identification
        /// </summary>
        [DeclareField("the resource identification")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the current migration point of the resource
        /// </summary>
        [DeclareField("the current migration point of the resource")]
        public string CurrentPoint { get; set; }

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
    }
}
