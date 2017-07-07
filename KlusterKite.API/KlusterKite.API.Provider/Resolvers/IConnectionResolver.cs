// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IConnectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The connection resolver public methods
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Attributes;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// The connection resolver public methods
    /// </summary>
    public interface IConnectionResolver
    {
        /// <summary>
        /// Resolves connection object mutation request
        /// </summary>
        /// <param name="nodeConnection">The node connection</param>
        /// <param name="request">The mutation request</param>
        /// <param name="field">
        /// The mutating field
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
        /// <returns>The <see cref="MutationResult{T}"/></returns>
        Task<JObject> ResolveMutation(
            object nodeConnection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);
    }
}