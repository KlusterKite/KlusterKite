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
        /// Gets or sets the list of descriptions of installed modules
        /// </summary>
        public List<ModuleDescription> Modules { get; set; }

        /// <summary>
        /// Gets or sets node's address
        /// </summary>
        public Address NodeAddress { get; set; }

        /// <summary>
        /// Gets or sets node template code
        /// </summary>
        public string NodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets current node template version
        /// </summary>
        public int NodeTemplateVersion { get; set; }

        /// <summary>
        /// Description of installed module
        /// </summary>
        public class ModuleDescription
        {
            /// <summary>
            /// Gets or sets name of module
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets version of module
            /// </summary>
            public string Version { get; set; }
        }
    }
}