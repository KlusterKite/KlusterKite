// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Template.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   A cluster node template description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System.Collections.Generic;
    using System.Linq;

    using ClusterKit.API.Attributes;
    using ClusterKit.API.Client.Converters;
    using ClusterKit.NodeManager.Launcher.Messages;

    using JetBrains.Annotations;

    /// <summary>
    /// A cluster node template description
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("Node template description", Name = "Template")]
    public class Template
    {
        /// <summary>
        /// Gets or sets the program readable node template name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The program readable node template name", IsKey = true)]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets akka configuration template for node
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The akka configuration template for node")]
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets list of container types to install node templates
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The list of container types to install node templates")]
        public List<string> ContainerTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets maximum number of working nodes that is reasonable for cluster
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The maximum number of working nodes that is reasonable for cluster")]
        public int? MaximumNeededInstances { get; set; }

        /// <summary>
        /// Gets or sets minimum number of working node type required for cluster to work
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The minimum number of working node type required for cluster to work")]
        public int MinimumRequiredInstances { get; set; }

        /// <summary>
        /// Gets or sets the human readable node template name
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The human readable node template name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the template description for other users
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The template description for other users")]
        public string Notes { get; set; }

        /// <summary>
        /// Gets or sets priority weight for service, when deciding witch template should be brought up
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The priority weight for service, when deciding witch template should be brought up")]
        public double Priority { get; set; }

        /// <summary>
        /// Gets or sets the list of package requirements
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The list of package requirements")]
        public List<PackageRequirement> PackageRequirements { get; set; } = new List<PackageRequirement>();

        /// <summary>
        /// Gets or sets the list of packages to install for current template
        /// </summary>
        [DeclareField("The list of packages to install for current template", Converter = typeof(DictionaryConverter<string, List<PackageDescription>>))]
        public Dictionary<string, List<PackageDescription>> PackagesToInstall { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether no other nodes from previous releases are compatible
        /// </summary>
        [UsedImplicitly]
        [DeclareField("A value indicating whether no other nodes from previous releases are compatible")]
        public bool ForceUpdate { get; set; }

        /// <summary>
        /// Clones current object
        /// </summary>
        /// <param name="configuration">
        /// The configuration replacement, if set
        /// </param>
        /// <param name="code">
        /// The code replacement, if set
        /// </param>
        /// <param name="containerTypes">
        /// The container types list replacement, if set
        /// </param>
        /// <param name="packagesList">
        /// The packages list replacement, if set
        /// </param>
        /// <param name="id">
        /// The id replacement, if set
        /// </param>
        /// <param name="maximumNeededInstances">
        /// The maximum needed instances replacement, if set
        /// </param>
        /// <param name="minimumRequiredInstances">
        /// The minimum required instances replacement, if set
        /// </param>
        /// <param name="priority">
        /// The priority replacement, if set
        /// </param>
        /// <param name="packageRequirements">
        /// the list of package requirements
        /// </param>
        /// <returns>
        /// The new instance of <see cref="NodeTemplate"/>.
        /// </returns>
        [UsedImplicitly]
        public Template Clone(
            string configuration = null,
            string code = null,
            List<string> containerTypes = null,
            string packagesList = null,
            int? id = null,
            int? maximumNeededInstances = null,
            int? minimumRequiredInstances = null,
            int? priority = null,
            List<PackageRequirement> packageRequirements = null)
        {
            return new Template
                       {
                           Configuration = configuration ?? this.Configuration,
                           Code = code ?? this.Code,
                           ContainerTypes = containerTypes ?? new List<string>(this.ContainerTypes),
                           PackageRequirements =
                               packageRequirements
                               ?? new List<PackageRequirement>(this.PackageRequirements.Select(r => r.Clone())),
                           MaximumNeededInstances = maximumNeededInstances ?? this.MaximumNeededInstances,
                           MinimumRequiredInstances = minimumRequiredInstances ?? this.MinimumRequiredInstances,
                           Priority = priority ?? this.Priority
                       };
        }

        /// <summary>
        /// The package requirement for the template
        /// </summary>
        [ApiDescription("The package requirement for the template", Name = "PackageRequirement")]
        public class PackageRequirement
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PackageRequirement"/> class.
            /// </summary>
            [UsedImplicitly]
            public PackageRequirement()
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="PackageRequirement"/> class.
            /// </summary>
            /// <param name="id">
            /// The id.
            /// </param>
            /// <param name="specificVersion">
            /// The specific version.
            /// </param>
            public PackageRequirement(string id, string specificVersion)
            {
                this.Id = id;
                this.SpecificVersion = specificVersion;
            }

            /// <summary>
            /// Gets or sets the Nuget package id
            /// </summary>
            [UsedImplicitly]
            [DeclareField("the Nuget package id", IsKey = true)]
            public string Id { get; set; }

            /// <summary>
            /// Gets or sets the required specific version (in case of null the default version for release will be used)
            /// </summary>
            [UsedImplicitly]
            [DeclareField("the required specific version (in case of null the default version for release will be used)")]
            public string SpecificVersion { get; set; }

            /// <summary>
            /// Creates a deep clone of the object
            /// </summary>
            /// <returns>The object's clone</returns>
            public PackageRequirement Clone()
            {
                return new PackageRequirement(this.Id, this.SpecificVersion);
            }
        }
    }
}
