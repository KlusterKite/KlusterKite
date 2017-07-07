// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestContext.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The full operation request description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Attributes
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The full operation request description
    /// </summary>
    public class RequestContext
    {
        /// <summary>
        /// Gets or sets the authentication description
        /// </summary>
        public AccessTicket Authentication { get; set; }

        /// <summary>
        /// Gets or sets the client / user remote address
        /// </summary>
        public string RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the request headers (if applicable)
        /// </summary>
        [CanBeNull]
        public Dictionary<string, string> Headers { get; set; }

        /// <summary>
        ///  Gets or sets the local uri request
        /// </summary>
        public string RequestedLocalUrl { get; set; }

        /// <summary>
        /// Gets or sets additional data, that can be updated on previous steps
        /// </summary>
        [CanBeNull]
        public JObject Data { get; set; } 
    }
}
