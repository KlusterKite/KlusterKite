// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UpdateClusterRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Performs the cluster update
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Performs the cluster update
    /// </summary>
    public class UpdateClusterRequest
    {
        /// <summary>
        /// Gets or sets the configuration id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}
