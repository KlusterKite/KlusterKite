// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseSetStableRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The set stable request.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using ClusterKit.Security.Attributes;

    /// <summary>
    /// The release set stable mark request.
    /// </summary>
    public class ReleaseSetStableRequest
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
        /// Gets or sets a value indicating whether selected release should be marked as stable or unstable
        /// </summary>
        public bool IsStable { get; set; }
    }
}
