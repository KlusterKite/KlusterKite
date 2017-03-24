// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionResolver.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Resolves requests to the connection
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.API.Provider.Resolvers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.API.Client.Attributes.Authorization;
    using ClusterKit.Security.Client;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves requests to the connection
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType",
        Justification = "Making use of static properties in generic classes")]
    internal class ConnectionResolver<T> : CollectionResolver<T>, IConnectionResolver
        where T : class, new()
    {
        /// <summary>
        /// A value indicating whether the type initialization process was completed
        /// </summary>
        private static bool isInitialized;

        /// <summary>
        /// Gets or sets the mutation result resolver
        /// </summary>
        private static ObjectResolver mutationResultResolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionResolver{T}"/> class.
        /// </summary>
        public ConnectionResolver()
        {
            InitializeType();
        }

        /// <inheritdoc />
        public Task<JObject> ResolveMutation(
            object nodeConnection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var connection = nodeConnection as INodeConnection<T>;
            if (connection == null)
            {
                return Task.FromResult<JObject>(null);
            }

            switch (request.FieldName)
            {
                case "create":
                    return this.MutationCreate(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                case "update":
                    return this.MutationUpdate(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                case "delete":
                    return this.MutationDelete(
                        connection,
                        request,
                        field,
                        context,
                        argumentsSerializer,
                        onErrorCallback);
                default:
                    return Task.FromResult<JObject>(null);
            }
        }

        /// <summary>
        /// Getting the query result
        /// </summary>
        /// <param name="source">The data source</param>
        /// <param name="request">The original request</param>
        /// <param name="filter">Filtering expression</param>
        /// <param name="sort">Sorting expression</param>
        /// <param name="limit">The maximum number of elements</param>
        /// <param name="offset">The number of the first element</param>
        /// <returns>The query result</returns>
        protected override Task<QueryResult<T>> GetQueryResult(
            object source,
            ApiRequest request,
            Expression<Func<T, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset)
        {
            var connection = source as INodeConnection<T>;
            return connection?.Query(filter, sort, limit, offset, request);
        }

        /// <summary>
        /// Performs final type initialization
        /// </summary>
        private static void InitializeType()
        {
            if (isInitialized)
            {
                return;
            }

            lock (LockObject)
            {
                if (isInitialized)
                {
                    return;
                }

                isInitialized = true;
                mutationResultResolver = new ObjectResolver<MutationResult<T>>();
            }
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
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
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationCreate(
            INodeConnection<T> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Create(newNode);
            var mutationCreate =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request,
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            SetLog(request, field, context, EnConnectionAction.Create);
            return mutationCreate;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
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
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationDelete(
            INodeConnection<T> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId?.ToObject(NodeMetaData.KeyProperty.PropertyType);

            if (id == null)
            {
                return null;
            }

            var result = await connection.Delete(id);
            var mutationDelete =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request,
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            if (result.Result != null)
            {
                mutationDelete.Add("__deletedId", JToken.FromObject(id));
            }

            SetLog(request, field, context, EnConnectionAction.Delete);
            return mutationDelete;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
        /// <param name="field">
        /// The connection field
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
        /// <returns>The resolved data</returns>
        private async Task<JObject> MutationUpdate(
            INodeConnection<T> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId?.ToObject(NodeMetaData.KeyProperty.PropertyType);
            if (id == null)
            {
                return null;
            }

            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Update(id, newNode, request);
            var mutationUpdate =
                (JObject)
                await mutationResultResolver.ResolveQuery(
                    result,
                    request,
                    field,
                    context,
                    argumentsSerializer,
                    onErrorCallback);
            if (result.Result != null)
            {
                if (!id.Equals(GetIdValue(result.Result)))
                {
                    mutationUpdate.Add("__deletedId", JToken.FromObject(id));
                }
            }

            SetLog(request, field, context, EnConnectionAction.Update);
            return mutationUpdate;
        }
    }
}