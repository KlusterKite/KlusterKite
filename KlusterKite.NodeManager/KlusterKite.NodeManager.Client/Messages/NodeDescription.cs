// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeDescription.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the NodeDescription type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Client.Messages
{
    using System;
    using System.Collections.Generic;

    using Akka.Actor;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.ApiSurrogates;

    /// <summary>
    /// Description of node configuration
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("Description of node configuration", Name = "NodeDescription")]
    public class NodeDescription
    {
        /// <summary>
        /// Gets or sets symbolic container type code
        /// </summary>
        [DeclareField("symbolic container type code")]
        public string ContainerType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current node is cluster leader
        /// </summary>
        [DeclareField("a value indicating whether the current node is cluster leader")]
        public bool IsClusterLeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether software and or configuration is obsolete and needed to be upgraded
        /// </summary>
        [DeclareField("a value indicating whether software and or configuration is obsolete and needed to be upgraded")]
        public bool IsObsolete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is complete description
        /// </summary>
        [UsedImplicitly]
        [DeclareField("a value indicating whether this is complete description")]
        public bool IsInitialized { get; set; }

        /// <summary>
        /// Gets or sets the list of roles, where current node is leader
        /// </summary>
        [DeclareField("the list of roles, where current node is leader")]
        public List<string> LeaderInRoles { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of descriptions of installed modules
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of descriptions of installed modules")]
        public List<Launcher.Messages.PackageDescription> Modules { get; set; }

        /// <summary>
        /// Gets or sets node's address
        /// </summary>
        [DeclareField("The node's address", Converter = typeof(AkkaAddressSurrogate.Converter))]
        public Address NodeAddress { get; set; }

        /// <summary>
        /// Gets or sets request id to indicate node instance startup
        /// </summary>
        [DeclareField("request id to indicate node instance startup", IsKey = true)]
        public Guid NodeId { get; set; }

        /// <summary>
        /// Gets or sets node template code
        /// </summary>
        [DeclareField("node template code")]
        public string NodeTemplate { get; set; }

        /// <summary>
        /// Gets or sets the installed release id
        /// </summary>
        [DeclareField("the installed release id")]
        public int ReleaseId { get; set; }

        /// <summary>
        /// Gets or sets the list of cluster roles
        /// </summary>
        [UsedImplicitly]
        [DeclareField("the list of cluster roles")]
        public List<string> Roles { get; set; }

        /// <summary>
        /// Gets or sets node start time
        /// </summary>
        [DeclareField("node start time")]
        public long StartTimeStamp { get; set; }
    }
}