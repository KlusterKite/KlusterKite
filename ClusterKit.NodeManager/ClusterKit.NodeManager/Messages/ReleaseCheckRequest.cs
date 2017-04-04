// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Makes the draft release check
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.Messages
{
    using ClusterKit.Security.Client;

    /// <summary>
    /// Makes the draft release check
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ReleaseCheckRequest
    {
        /// <summary>
        /// Gets or sets the release id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}