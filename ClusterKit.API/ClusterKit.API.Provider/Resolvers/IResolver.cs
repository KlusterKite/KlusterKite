// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves api requests for an object
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves api requests for an object
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolves API request to object
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="request">
        /// The request to this object as a field of parent object.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<JToken> ResolveQuery(object source, ApiRequest request, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback);
    }
}
