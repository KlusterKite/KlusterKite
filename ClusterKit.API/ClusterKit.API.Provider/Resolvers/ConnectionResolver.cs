// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ConnectionResolver type.
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
    /// Resolves requests to the connection
    /// </summary>
    public abstract class ConnectionResolver : IResolver
    {
        /// <inheritdoc />
        public Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            throw new NotImplementedException();
        }
    }
}
