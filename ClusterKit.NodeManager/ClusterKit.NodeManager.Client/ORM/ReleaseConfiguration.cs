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

    using ClusterKit.API.Attributes;
    using ClusterKit.NodeManager.Launcher.Messages;

    /// <summary>
    /// The release configuration description
    /// </summary>
    [ApiDescription("The release configuration description", Name = "ReleaseConfiguration")]
    public class ReleaseConfiguration
    { 
        /// <summary>
        /// Gets or sets the list of available packages with their versions
        /// </summary>
        [DeclareField("the list of available packages with their versions")]
        public List<PackageDescription> Packages { get; set; }

        /// <summary>
        /// Gets or sets the list of configured node templates
        /// </summary>
        [DeclareField("the list of configured node templates")]
        public List<Template> NodeTemplates { get; set; }

        /// <summary>
        /// Gets or sets the seed addresses
        /// </summary>
        [DeclareField("the list of seed addresses")]
        public List<string> SeedAddresses { get; set; }

        /// <summary>
        /// Gets or sets the list of nuget feeds
        /// </summary>
        [DeclareField("the list of nuget feeds")]
        public List<NugetFeed> NugetFeeds { get; set; }
    }
}
