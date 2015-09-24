// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptionResponse.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The message, that is sent as response to <seealso cref="WebDescriptionRequest" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Client.Messages
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The message, that is sent as response to <seealso cref="WebDescriptionRequest"/>
    /// </summary>
    [UsedImplicitly]
    public class WebDescriptionResponse
    {
        /// <summary>
        /// Gets or sets the port, where web service is listening connections
        /// </summary>
        [UsedImplicitly]
        public int ListeningPort { get; set; }

        /// <summary>
        /// Gets or sets the the list of services.
        /// </summary>
        /// <remarks>
        /// It doesn't supposed (but is not prohibited) that this should be public service hostname.
        /// It's just used to distinguish services with identical url paths to be correctly published on frontend web servers.
        /// </remarks>
        [UsedImplicitly]
        public Dictionary<string, string> ServiceNames { get; set; }
    }
}