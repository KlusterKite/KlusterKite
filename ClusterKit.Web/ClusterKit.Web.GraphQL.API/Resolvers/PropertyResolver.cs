// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves object property value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.GraphQL.API.Resolvers
{
    using System.Threading.Tasks;

    using ClusterKit.Security.Client;
    using ClusterKit.Web.GraphQL.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves object property value
    /// </summary>
    public abstract class PropertyResolver
    {
        /// <summary>
        /// Resolves object property value
        /// </summary>
        /// <param name="source">
        /// The object data
        /// </param>
        /// <param name="query">
        /// This field request.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The json serializer used to deserialize field arguments
        /// </param>
        /// <returns>
        /// The property value
        /// </returns>
        public abstract Task<JToken> Resolve(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer);
    }
}
