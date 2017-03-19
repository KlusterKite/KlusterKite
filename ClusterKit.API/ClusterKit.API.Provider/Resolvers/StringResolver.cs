// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves value via <see cref="object.ToString" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves value via <see cref="object.ToString"/>
    /// </summary>
    [UsedImplicitly]
    public class StringResolver : IResolver
    {
        /// <inheritdoc />
        public Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            return Task.FromResult<JToken>(new JValue(source?.ToString()));
        }

        /// <inheritdoc />
        public ApiType GetElementType()
        {
            return null;
        }
    }
}