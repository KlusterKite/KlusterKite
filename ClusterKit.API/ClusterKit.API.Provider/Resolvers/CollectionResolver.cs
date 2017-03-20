// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CollectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the CollectionResolver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves the collections
    /// </summary>
    public class CollectionResolver : IResolver
    {
        /// <summary>
        /// Resolver for the collection element
        /// </summary>
        private readonly IResolver elementResolver;

        /// <inheritdoc />
        public CollectionResolver(IResolver elementResolver)
        {
            this.elementResolver = elementResolver;
        }

        /// <inheritdoc />
        public async Task<JToken> ResolveQuery(object source, ApiRequest request, ApiField apiField, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback)
        {
            if (source == null)
            {
                return JValue.CreateNull();
            }

            var collection = source as IEnumerable;
            if (collection == null)
            {
                onErrorCallback?.Invoke(new InvalidOperationException($"{source.GetType().FullName} is not a collection"));
                return JValue.CreateNull();
            }

            var result = new JArray();
            foreach (var value in collection)
            {
                result.Add(await this.elementResolver.ResolveQuery(value, request, apiField, context, argumentsSerializer, onErrorCallback));
            }

            return result;
        }

        /// <inheritdoc />
        public ApiType GetElementType() => this.elementResolver.GetElementType();
    }
}
