// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetFeed.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Link to the nuget feed to download updates
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Client.ORM
{
    using System;

    using ClusterKit.API.Attributes;

    using JetBrains.Annotations;

    /// <summary>
    /// Link to the nuget feed to download updates
    /// </summary>
    [ApiDescription("The link to the nuget feed to download updates", Name = "NugetFeed")]
    public class NugetFeed
    {
        /// <summary>
        /// Type of NuGet feed
        /// </summary>
        [ApiDescription("The type of nuget feed", Name = "EnFeedType")]
        public enum EnFeedType
        {
            /// <summary>
            /// Public feed used to download third party packages
            /// </summary>
            [UsedImplicitly]
            Public = 0,

            /// <summary>
            /// Private feed is checked against updates and can be source of cluster update events
            /// </summary>
            [UsedImplicitly]
            Private = 1
        }

        /// <summary>
        /// Gets or sets seed url address
        /// </summary>
        [DeclareField("The seed url address", IsKey = true)]
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets user password for basic authentication
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The user password for basic authentication")]
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets type of feed
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The  type of feed")]
        public EnFeedType Type { get; set; }

        /// <summary>
        /// Gets or sets username for basic authentication
        /// </summary>
        [UsedImplicitly]
        [DeclareField("The username for basic authentication")]
        public string UserName { get; set; }
    }
}