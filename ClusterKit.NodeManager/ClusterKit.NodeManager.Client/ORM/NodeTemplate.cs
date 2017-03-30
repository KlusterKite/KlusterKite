// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeTemplate.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Node template description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Xml.Serialization;

    using ClusterKit.API.Client.Attributes;
    using ClusterKit.Data.CRUD;

    using JetBrains.Annotations;

    using Newtonsoft.Json;

    /// <summary>
    /// Node template description
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("Node template description", Name = "NodeTemplate")]
    public class NodeTemplate : IObjectWithId<int>
    {
        /// <summary>
        /// Gets or sets the program readable node template name
        /// </summary>
        [Required]
        [DeclareField("The program readable node template name")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets akka configuration template for node
        /// </summary>
        [DeclareField("The akka configuration template for node")]
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets list of container types to install node templates
        /// </summary>
        [NotMapped]
        [DeclareField("The list of container types to install node templates")]
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
        [DeclareField("The unique template identification number", IsKey = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets maximum number of working nodes that is reasonable for cluster
        /// </summary>
        [DeclareField("The maximum number of working nodes that is reasonable for cluster")]
        public int? MaximumNeededInstances { get; set; }

        /// <summary>
        /// Gets or sets minimum number of working node type required for cluster to work
        /// </summary>
        [DeclareField("The minimum number of working node type required for cluster to work")]
        public int MinimumRequiredInstances { get; set; }

        /// <summary>
        /// Gets or sets the human readable node template name
        /// </summary>
        [Required]
        [DeclareField("The human readable node template name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets list of nuget packages to install (along with there dependencies)
        /// </summary>
        [NotMapped]
        [DeclareField("The list of nuget packages to install (along with there dependencies)")]
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
        [DeclareField("The priority weight for service, when deciding witch template should be brought up")]
        public double Priority { get; set; }

        /// <summary>
        /// Gets or sets the template version
        /// </summary>
        [DeclareField("The template version")]
        public int Version { get; set; }

        /// <inheritdoc />
        int IObjectWithId<int>.GetId()
        {
            return this.Id;
        }
    }
}