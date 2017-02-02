// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RequestDescription.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The full operation request description
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Security.Client
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The full operation request description
    /// </summary>
    public class RequestDescription
    {
        /// <summary>
        /// Gets or sets the authentication description
        /// </summary>
        public UserSession Authentication { get; set; }

        /// <summary>
        /// Gets or sets the client / user remote address
        /// </summary>
        public string RemoteAddress { get; set; }

        /// <summary>
        /// Gets or sets the request headers (if applicable)
        /// </summary>
        [CanBeNull]
        public Dictionary<string, string> Headers { get; set; }
    }
}
