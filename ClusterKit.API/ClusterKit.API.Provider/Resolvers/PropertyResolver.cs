// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves object property value
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves object property value
    /// </summary>
    public abstract class PropertyResolver
    {
        /// <summary>
        /// Resolves and extracts object property value
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
        /// <param name="onErrorCallback">
        /// The method that will be called in case of errors
        /// </param>
        /// <returns>
        /// The property value
        /// </returns>
        public abstract Task<JToken> Resolve(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback);

        /// <summary>
        /// Gets the object property value
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
        public abstract Task<object> GetValue(object source, ApiRequest query, RequestContext context, JsonSerializer argumentsSerializer);

        /// <summary>
        /// Gets the object property value according to property resolving path
        /// </summary>
        /// <param name="source">
        /// The object data
        /// </param>
        /// <param name="path">
        /// the request path
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
        public abstract Task<object> GetValueRecursive(object source, Queue<ApiRequest> path, RequestContext context, JsonSerializer argumentsSerializer);
    }
}
