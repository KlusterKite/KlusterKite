// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueriApiRequest.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The read-only query api request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Client
{
    using System.Collections.Generic;

    using ClusterKit.Security.Client;

    /// <summary>
    /// The read-only query api request
    /// </summary>
    public class QueriApiRequest 
    {
        /// <summary>
        /// Gets or sets the list of queried fields
        /// </summary>
        public List<ApiRequest> Fields { get; set; }

        /// <summary>
        /// Gets or sets the request context
        /// </summary>
        public RequestContext Context { get; set; }
    }
}
