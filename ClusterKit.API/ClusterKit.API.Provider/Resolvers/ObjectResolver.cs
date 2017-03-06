// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the ObjectResolver type.
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
    /// Base class to resolve requests to objects 
    /// </summary>
    public abstract class ObjectResolver : IResolver
    {
        /// <summary>
        /// Gets a value of objects property according to API request
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="request">
        /// The api request.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>The value</returns>
        public abstract Task<ResolvePropertyResult> ResolvePropertyValue(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback);

        /// <summary>
        /// Resolves API request to object
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="request">
        /// The request to this object as a field of parent object.
        /// </param>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <param name="argumentsSerializer">
        /// The arguments serializer.
        /// </param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<JObject> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            if (request?.Fields == null || request.Fields.Count == 0)
            {
                return null;
            }

            var result = new JObject();

            var mergeSettings = new JsonMergeSettings
                                    {
                                        MergeArrayHandling = MergeArrayHandling.Concat,
                                        MergeNullValueHandling = MergeNullValueHandling.Merge
                                    };

            foreach (var fieldRequest in request.Fields)
            {
                result.Merge(
                    await this.ResolveFieldQuery(source, context, argumentsSerializer, onErrorCallback, fieldRequest),
                    mergeSettings);
            }

            if (request.FieldName != null)
            {
                var requestDescription = new JObject { { "f", request.FieldName } };
                if (request.Arguments != null)
                {
                    requestDescription.Add("a", request.Arguments);
                }

                result.Add("__request", requestDescription);
            }

            return result;
        }

        /// <inheritdoc />
        async Task<JToken> IResolver.ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            return await this.ResolveQuery(source, request, context, argumentsSerializer, onErrorCallback);
        }

        /// <summary>
        /// Resolves query for a field
        /// </summary>
        /// <param name="source">The source data object</param>
        /// <param name="context">The request context</param>
        /// <param name="argumentsSerializer">The arguments serializer</param>
        /// <param name="onErrorCallback">
        /// The on error callback.
        /// </param>
        /// <param name="fieldRequest">The field request</param>
        /// <returns>The resolved request</returns>
        private async Task<JObject> ResolveFieldQuery(
            object source,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback,
            ApiRequest fieldRequest)
        {
            var fieldName = fieldRequest.Alias ?? fieldRequest.FieldName;

            try
            {
                var propertyValue = await this.ResolvePropertyValue(
                                        source,
                                        fieldRequest,
                                        context,
                                        argumentsSerializer,
                                        onErrorCallback);

                var resolvedProperty = propertyValue?.Value == null
                                           ? JValue.CreateNull()
                                           : await propertyValue.Resolver.ResolveQuery(
                                                 propertyValue.Value,
                                                 fieldRequest,
                                                 context,
                                                 argumentsSerializer,
                                                 onErrorCallback);
                return new JObject { { fieldName, resolvedProperty } };
            }
            catch (Exception exception)
            {
                onErrorCallback?.Invoke(exception);
                return new JObject { { fieldName, JValue.CreateNull() } };
            }
        }

        /// <summary>
        /// The result of resolve property operation
        /// </summary>
        [UsedImplicitly]
        public class ResolvePropertyResult
        {
            /// <summary>
            /// Gets or sets the property value
            /// </summary>
            [UsedImplicitly]
            public object Value { get; set; }

            /// <summary>
            /// Gets or sets the property value resolver
            /// </summary>
            [UsedImplicitly]
            public IResolver Resolver { get; set; }
        }
    }
}
