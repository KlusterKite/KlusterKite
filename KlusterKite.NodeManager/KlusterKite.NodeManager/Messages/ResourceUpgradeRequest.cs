﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ResourceUpgradeRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The request to update resources
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.NodeManager.Client.Messages.Migration;

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