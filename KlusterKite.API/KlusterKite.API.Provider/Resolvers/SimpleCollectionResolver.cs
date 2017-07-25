// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SimpleCollectionResolver.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the SimpleCollectionResolver type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Provider.Resolvers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using KlusterKite.API.Client;
    using KlusterKite.Security.Attributes;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves the collections without any filtering / sorting
    /// </summary>
    public class SimpleCollectionResolver : IResolver
    {
        /// <summary>
        /// Resolver for the collection element
        /// </summary>
        private readonly IResolver elementResolver;

        /// <inheritdoc />
        public SimpleCollectionResolver(IResolver elementResolver)
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

        /// <inheritdoc />
        public IEnumerable<ApiField> GetTypeArguments()
        {
            yield break;
        }
    }
}
