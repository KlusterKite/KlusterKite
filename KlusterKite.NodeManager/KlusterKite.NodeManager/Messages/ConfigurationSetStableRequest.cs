// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationSetStableRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The set stable request.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The configuration set stable mark request.
    /// </summary>
    public class ConfigurationSetStableRequest
    {
        /// <summary>
        /// Gets or sets the configuration id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether selected configuration should be marked as stable or unstable
        /// </summary>
        public bool IsStable { get; set; }
    }
}
