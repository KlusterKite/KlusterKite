// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QueryApiRequest.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The read-only query api request
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Client
{
    using System.Collections.Generic;

    using KlusterKite.Security.Attributes;

    /// <summary>
    /// The read-only query api request
    /// </summary>
    public class QueryApiRequest 
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
