// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IResolver.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves api requests for an object
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
    /// Resolves api requests for an object
    /// </summary>
    public interface IResolver
    {
        /// <summary>
        /// Resolves API request to object
        /// </summary>
        /// <param name="source">
        ///     The source.
        /// </param>
        /// <param name="request">
        ///     The request to this object as a field of parent object.
        /// </param>
        /// <param name="apiField">
        /// The container field description
        /// </param>
        /// <param name="context">
        ///     The context.
        /// </param>
        /// <param name="argumentsSerializer">
        ///     The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        ///     The on error callback.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            ApiField apiField,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);

        /// <summary>
        /// Gets the resolved api type of resolved element
        /// </summary>
        /// <returns>The api type</returns>
        ApiType GetElementType();

        /// <summary>
        /// Gets the list of arguments that are supported by resolver itself (not the original object method arguments)
        /// </summary>
        /// <returns>The list of additional arguments</returns>
        IEnumerable<ApiField> GetTypeArguments();
    }
}
