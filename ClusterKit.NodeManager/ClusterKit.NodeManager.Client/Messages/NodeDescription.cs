// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the NodeDescription type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.Messages
{
    using System;
    using System.Collections.Generic;

    using Akka.Actor;

    using JetBrains.Annotations;

    /// <summary>
    /// Description of node configuration
    /// </summary>
    [UsedImplicitly]
    public class NodeDescription
    {
        /// <summary>
        /// Gets or sets symbolic container type code
        /// </summary>
        public string ContainerType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether software and or configuration is obsolete and needed to be upgraded
        /// </summary>
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets the list of descriptions of installed modules
        /// </summary>
        public List<PackageDescription> Modules { get; set; }

        /// <summary>
        /// Gets or sets node's address
        /// </summary>
        public Address NodeAddress { get; set; }

        /// <summary>
        /// Gets or sets request id to indicate node instance startup
        /// </summary>
        public Guid NodeId { get; set; }

        /// <summary>
        /// Gets or sets node template code
        /// </summary>
        public string NodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets current node template version
        /// </summary>
        public int NodeTemplateVersion { get; set; }

        /// <summary>
        /// Gets or sets node start time
        /// </summary>
        public long StartTimeStamp { get; set; }
    }
}