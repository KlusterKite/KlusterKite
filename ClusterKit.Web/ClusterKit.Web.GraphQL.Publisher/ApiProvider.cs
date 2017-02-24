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

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Client;

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
    }
}
