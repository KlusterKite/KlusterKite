// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ForwarderResolver.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves value for forwarded fields
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using KlusterKite.API.Client;
    using KlusterKite.Security.Attributes;

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

        /// <inheritdoc />
        public IEnumerable<ApiField> GetTypeArguments()
        {
            yield break;
        }
    }
}
