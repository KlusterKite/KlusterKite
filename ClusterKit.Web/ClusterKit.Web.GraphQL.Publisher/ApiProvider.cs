// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApiProvider.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The description of the API provider
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.Publisher
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The description of the API provider
    /// </summary>
    public abstract class ApiProvider
    {
        /// <summary>
        /// Gets or sets current provider API description
        /// </summary>
        public ApiDescription Description { get; set; }

        /// <summary>
        /// Retrieves specified data for api request
        /// </summary>
        /// <param name="requests">
        /// The request
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The resolved data
        /// </returns>
        public abstract Task<JObject> GetData(List<ApiRequest> requests, RequestContext context);

        /// <summary>
        /// Searches for the connection node
        /// </summary>
        /// <param name="id">
        /// The node id
        /// </param>
        /// <param name="path">
        /// The node connection path in API
        /// </param>
        /// <param name="context">
        /// The request context.
        /// </param>
        /// <returns>
        /// The serialized node value
        /// </returns>
        public abstract Task<JObject> SearchNode(string id, List<RequestPathElement> path, RequestContext context);
    }
}
