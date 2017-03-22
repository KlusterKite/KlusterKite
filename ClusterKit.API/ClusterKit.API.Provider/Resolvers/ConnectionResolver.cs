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
    /// <typeparam name="TId">The type of node id</typeparam>
    [SuppressMessage("ReSharper", "StaticMemberInGenericType",
        Justification = "Making use of static properties in generic classes")]
    public class ConnectionResolver<T, TId> : CollectionResolver<T>, IConnectionResolver
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
        /// Initializes a new instance of the <see cref="ConnectionResolver{T,TId}"/> class.
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
            var connection = nodeConnection as INodeConnection<T, TId>;
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

        /// <inheritdoc />
        public override async Task<object> GetNodeById(object nodeConnection, string id)
        {
            var connection = nodeConnection as INodeConnection<T, TId>;
            if (connection == null)
            {
                return null;
            }

            TId realId;
            try
            {
                realId = JsonConvert.DeserializeObject<TId>(id);
            }
            catch
            {
                return null;
            }

            return await connection.GetById(realId);
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
            var connection = source as INodeConnection<T, TId>;
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
            INodeConnection<T, TId> connection,
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
            INodeConnection<T, TId> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
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
                mutationDelete.Add("__deletedId", JsonConvert.SerializeObject(id));
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
            INodeConnection<T, TId> connection,
            ApiRequest request,
            ApiField field,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
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
                if (id != null && !id.Equals(connection.GetId(result.Result)))
                {
                    mutationUpdate.Add("__deletedId", JsonConvert.SerializeObject(id));
                }
            }

            SetLog(request, field, context, EnConnectionAction.Update);
            return mutationUpdate;
        }
    }
}