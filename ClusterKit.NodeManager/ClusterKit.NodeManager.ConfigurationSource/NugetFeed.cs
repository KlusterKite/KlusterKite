// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetFeed.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Link to the nuget feed to download updates
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    /// <summary>
    /// Link to the nuget feed to download updates
    /// </summary>
    public class NugetFeed
    {
        /// <summary>
        /// Type of NuGet feed
        /// </summary>
        public enum EnFeedType
        {
            /// <summary>
            /// Public feed used to download third party packages
            /// </summary>
            Public = 0,

            /// <summary>
            /// Private feed is checked against updates and can be source of cluster update events
            /// </summary>
            Private = 1
        }

        /// <summary>
        /// Gets or sets seed url address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Gets or sets unique address identification number
        /// </summary>
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets user password for basic authentication
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets type of feed
        /// </summary>
        public EnFeedType Type { get; set; }

        /// <summary>
        /// Gets or sets username for basic authentication
        /// </summary>
        public string UserName { get; set; }
    }
}