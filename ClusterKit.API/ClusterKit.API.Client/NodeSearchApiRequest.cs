// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NodeSearchApiRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The api query for node search
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System.Collections.Generic;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The api query for node search
    /// </summary>
    public class NodeSearchApiRequest
    {
        /// <summary>
        /// Gets or sets the id value
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the path to the connection
        /// </summary>
        public List<ApiRequest> Path { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}