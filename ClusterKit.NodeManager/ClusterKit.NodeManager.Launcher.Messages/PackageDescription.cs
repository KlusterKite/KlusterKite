// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PackageDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Short description of NuGet package
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Launcher.Messages
{
    using NuGet;

    /// <summary>
    /// Short description of NuGet package
    /// </summary>
    public class PackageDescription 
    {
        /// <summary>
        /// Gets or sets build time of assembly (if specified)
        /// </summary>
        public string BuildDate { get; set; }

        /// <summary>
        /// Gets or sets the package Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the package latest version
        /// </summary>
        public string Version { get; set; }
    }
}