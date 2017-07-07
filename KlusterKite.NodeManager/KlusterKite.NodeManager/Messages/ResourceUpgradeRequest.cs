// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceUpgradeRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The request to update resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using System.Collections.Generic;
    
    using ClusterKit.API.Attributes;
    using ClusterKit.NodeManager.Client.Messages.Migration;

    using JetBrains.Annotations;

    /// <summary>
    /// The request to update resources
    /// </summary>
    [UsedImplicitly]
    [ApiDescription("The request to update resources", Name = "ResourceUpgradeRequest")]
    public class ResourceUpgradeRequest
    {
        /// <summary>
        /// Gets or sets the list of resources to update
        /// </summary>
        [DeclareField("the list of resources to update")]
        public List<ResourceUpgrade> Resources { get; set; }
    }
}