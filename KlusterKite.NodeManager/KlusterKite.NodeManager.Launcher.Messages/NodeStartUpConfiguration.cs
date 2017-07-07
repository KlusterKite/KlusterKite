// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeStartUpConfiguration.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Set of instructions needed to create new node
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Launcher.Messages
{
    using System.Collections.Generic;

    /// <summary>
    /// Set of instructions needed to create new node
    /// </summary>
    public class NodeStartUpConfiguration
    {
        /// <summary>
        /// Gets or sets top level akka configuration
        /// </summary>
        public string Configuration { get; set; }

        /// <summary>
        /// Gets or sets node template code name
        /// </summary>
        public string NodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the node configuration release id
        /// </summary>
        public int ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets list of packages to install on node
        /// </summary>
        public List<PackageDescription> Packages { get; set; }

        /// <summary>
        /// Gets or sets NuGet feed url to download packages
        /// </summary>
        public string PackageSource { get; set; }

        /// <summary>
        /// Gets or sets list of akka cluster seeds to configure akka cluster node
        /// </summary>
        public List<string> Seeds { get; set; }
    }
}