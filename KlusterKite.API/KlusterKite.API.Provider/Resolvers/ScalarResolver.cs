// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScalarResolver.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves value for simple objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using KlusterKite.API.Client;
    using KlusterKite.Security.Attributes;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves value for simple objects
    /// </summary>
    /// <typeparam name="TScalar">The type of scalar</typeparam>
    [UsedImplicitly]
    public class ScalarResolver<TScalar> : IResolver
    {
        /// <inheritdoc />
        public Task<JToken> ResolveQuery(object source, ApiRequest request, ApiField apiField, RequestContext context, JsonSerializer argumentsSerializer, Action<Exception> onErrorCallback)
        {
            return Task.FromResult<JToken>(new JValue((TScalar)source));
        }

        /// <inheritdoc />
        public ApiType GetElementType()
        {
            return null;
        }

        /// <inheritdoc />
        public IEnumerable<ApiField> GetTypeArguments()
        {
            yield break;
        }
    }
}