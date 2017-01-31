// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Node template description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Xml.Serialization;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// Node template description
    /// </summary>
    [UsedImplicitly]
    public class NodeTemplate
    {
        /// <summary>
        /// Gets or sets the program readable node template name
        /// </summary>
        [Required]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets akka configuration template for node
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets list of container types to install node templates
        /// </summary>
        [NotMapped]
        public List<string> ContainerTypes { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets stored list of container types to install node template
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string ContainerTypesList
        {
            get
            {
                return this.ContainerTypes == null ? string.Empty : string.Join("; ", this.ContainerTypes);
            }

            set
            {
                this.ContainerTypes = value?.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets unique template identification number
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets maximum number of working nodes that is reasonable for cluster
        /// </summary>
        public int? MaximumNeededInstances { get; set; }

        /// <summary>
        /// Gets or sets minimum number of working node type required for cluster to work
        /// </summary>
        public int MinimumRequiredInstances { get; set; }

        /// <summary>
        /// Gets or sets the human readable node template name
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of nuget packages to install (along with there dependencies)
        /// </summary>
        [NotMapped]
        public List<string> Packages { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets stored list of nuget packages to install (along with there dependencies)
        /// </summary>
        [JsonIgnore]
        [XmlIgnore]
        [UsedImplicitly]
        public string PackagesList
        {
            get
            {
                return this.Packages == null ? string.Empty : string.Join("; ", this.Packages);
            }

            set
            {
                this.Packages = value?.Split(new[] { "; " }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets priority weight for service, when deciding witch template should be brought up
        /// </summary>
        public double Priority { get; set; }

        /// <summary>
        /// Gets or sets the template version
        /// </summary>
        public int Version { get; set; }
    }
}