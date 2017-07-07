// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseCheckRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Makes the draft release check
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using KlusterKite.Security.Attributes;

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