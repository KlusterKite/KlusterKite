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
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        public List<NodeTemplate> NodeTemplates { get; set; }

        /// <summary>
        /// Gets or sets the list of configured cluster migrator templates
        /// </summary>
        [DeclareField("the list of configured cluster migrator templates")]
        public List<MigratorTemplate> MigratorTemplates { get; set; }

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

        /// <summary>
        /// Get the united list of <see cref="NodeTemplates"/> and <see cref="MigratorTemplates"/>
        /// </summary>
        /// <returns>The list of templates</returns>
        public IEnumerable<ITemplate> GetAllTemplates() => this.NodeTemplates.Cast<ITemplate>().Union(this.MigratorTemplates);
    }
}
