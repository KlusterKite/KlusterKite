// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ScalarResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves value for simple objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

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
        public Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            return Task.FromResult<JToken>(new JValue((TScalar)source));
        }
    }
}