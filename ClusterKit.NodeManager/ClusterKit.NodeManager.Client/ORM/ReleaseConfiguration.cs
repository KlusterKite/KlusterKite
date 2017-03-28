// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseConfiguration.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The release configuration description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System.Collections.Generic;

    using ClusterKit.API.Client.Attributes;
    using ClusterKit.NodeManager.Client.ApiSurrogates;

    /// <summary>
    /// The release configuration description
    /// </summary>
    [ApiDescription(Description = "The release configuration description", Name = "ReleaseConfiguration")]
    public class ReleaseConfiguration
    { 
        /// <summary>
        /// Gets or sets the list of available packages with their versions
        /// </summary>
        [DeclareField(Description = "the list of available packages with their versions")]
        public List<PackageDescriptionSurrogate> Packages { get; set; }

        /// <summary>
        /// Gets or sets the list of configured node templates
        /// </summary>
        [DeclareField(Description = "the list of configured node templates")]
        public List<NodeTemplate> NodeTemplates { get; set; }
    }
}
