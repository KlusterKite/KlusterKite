// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiDiscoverResponse.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The response message for <see cref="ApiDiscoverRequest" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client.Messages
{
    using Akka.Actor;

    /// <summary>
    /// The response message for <see cref="ApiDiscoverRequest"/>
    /// </summary>
    public class ApiDiscoverResponse
    {
        /// <summary>
        /// Gets or sets the api description
        /// </summary>
        public ApiDescription Description { get; set; }

        /// <summary>
        /// Gets or sets the handler actor reference
        /// </summary>
        public IActorRef Handler { get; set; }
    }
}
