// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebDescriptionResponse.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The message, that is sent as response to <seealso cref="WebDescriptionRequest" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Web.Client.Messages
{
    using System.Collections.Generic;

    using JetBrains.Annotations;

    /// <summary>
    /// The message, that is sent as response to <seealso cref="WebDescriptionRequest"/>
    /// </summary>
    [UsedImplicitly]
    public struct WebDescriptionResponse
    {
        /// <summary>
        /// Gets or sets collection of published services
        /// </summary>
        public IReadOnlyCollection<ServiceDescription> Services { get; set; }
    }
}