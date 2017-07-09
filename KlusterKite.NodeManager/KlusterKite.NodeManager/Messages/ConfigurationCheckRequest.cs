// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationCheckRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Makes the draft configuration check
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.Messages
{
    using KlusterKite.Security.Attributes;

    /// <summary>
    /// Makes the draft configuration check
    /// </summary>
    /// <returns>The mutation result</returns>
    public class ConfigurationCheckRequest
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