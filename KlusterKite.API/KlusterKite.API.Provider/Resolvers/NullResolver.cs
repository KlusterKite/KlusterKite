// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NullResolver.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Empty resolver that always return null
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
    /// Empty resolver that always return null
    /// </summary>
    public class NullResolver : IResolver
    {
        /// <inheritdoc />
        public Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            ApiField apiField,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            return Task.FromResult<JToken>(null);
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
