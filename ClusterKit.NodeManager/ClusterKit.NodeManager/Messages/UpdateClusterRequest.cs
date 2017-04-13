// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateClusterRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Performs the cluster update
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using ClusterKit.NodeManager.Client.ORM;
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// Performs the cluster update
    /// </summary>
    public class UpdateClusterRequest
    {
        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }

        /// <summary>
        /// Gets or sets the new state of current release
        /// </summary>
        public EnReleaseState CurrentReleaseState { get; set; }
    }
}
