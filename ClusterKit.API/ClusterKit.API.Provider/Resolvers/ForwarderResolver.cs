// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ForwarderResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves value for forwarded fields
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
    /// Resolves value for forwarded fields
    /// </summary>
    public class ForwarderResolver : IResolver
    {
        /// <summary>
        /// The resolved property type
        /// </summary>
        private readonly ApiType resolvedType;

        /// <inheritdoc />
        public ForwarderResolver(ApiType resolvedType)
        {
            this.resolvedType = resolvedType;
        }

        /// <inheritdoc />
        public Task<JToken> ResolveQuery(object source, ApiRequest request, ApiField apiField, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback)
        {
            var result = source as JToken;
            return Task.FromResult(result ?? JValue.CreateNull());
        }

        /// <inheritdoc />
        public ApiType GetElementType() => this.resolvedType;
    }
}
