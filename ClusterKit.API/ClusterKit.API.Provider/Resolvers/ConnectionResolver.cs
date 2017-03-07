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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.API.Client;
    using ClusterKit.Security.Client;

    using JetBrains.Annotations;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Resolves requests to the connection
    /// </summary>
    /// <typeparam name="T">The type of node</typeparam>
    /// <typeparam name="TId">The type of node id</typeparam>
    [UsedImplicitly]
    public abstract class ConnectionResolver<T, TId> : IResolver, IConnectionResolver 
        where T : class, new() 
    {
        /// <summary>
        /// Gets the node object resolver
        /// </summary>
        public abstract IResolver NodeResolver { get; }

        /// <summary>
        /// Gets the mutation result resolver
        /// </summary>
        public abstract IResolver MutationResultResolver { get; }

        /// <inheritdoc />
        public virtual async Task<JToken> ResolveQuery(
            object source,
            ApiRequest request,
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var connection = source as INodeConnection<T, TId>;
            if (connection == null)
            {
                onErrorCallback?.Invoke(new Exception("Source is not a node connection"));
                return JValue.CreateNull();
            }

            var arguments = (JObject)request.Arguments;
            var filterArgument = arguments?.Property("filter")?.Value as JObject;
            var sortArgument = arguments?.Property("sort")?.Value as JArray;
            var limit = (int?)(arguments?.Property("limit")?.Value as JValue);
            var offset = (int?)(arguments?.Property("offset")?.Value as JValue);

            var filter = filterArgument != null ? this.CreateFilter(filterArgument) : null;
            var sort = sortArgument != null ? this.CreateSort(sortArgument) : null;
            var items = await connection.Query(filter, sort, limit, offset);

            if (items == null)
            {
                return JValue.CreateNull();
            }

            var result = new JObject();
            foreach (var requestField in request.Fields)
            {
                switch (requestField.FieldName)
                {
                    case "count":
                        result.Add(requestField.Alias ?? requestField.FieldName, new JValue(items.Count));
                        break;
                    case "items":
                        {
                            var itemsValue = await new CollectionResolver(this.NodeResolver).ResolveQuery(
                                                 items.Items,
                                                 requestField,
                                                 context,
                                                 argumentsSerializer,
                                                 onErrorCallback);
                            result.Add(requestField.Alias ?? requestField.FieldName, itemsValue);
                        }

                        break;
                }
            }

            if (request.FieldName != null)
            {
                var requestDescription = new JObject { { "f", request.FieldName } };
                if (request.Arguments != null)
                {
                    var reservedNames = new[] { "filter", "sort", "limit", "offset" };
                    var requestArguments =
                        ((JObject)request.Arguments).Properties().Where(p => !reservedNames.Contains(p.Name)).ToList();
                    if (requestArguments.Count > 0)
                    {
                        var requestArgumentsObject = new JObject();
                        requestArguments.ForEach(ra => requestArgumentsObject.Add(ra.Name, ra.Value));
                        requestDescription.Add("a", requestArgumentsObject);
                    }
                }

                result.Add("__request", requestDescription);
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<object> GetNodeById(object nodeConnection, string id)
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

        /// <inheritdoc />
        public Task<JObject> ResolveMutation(
            object nodeConnection, 
            ApiRequest request,
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
                    return this.MutationCreate(connection, request, context, argumentsSerializer, onErrorCallback);
                case "update":
                    return this.MutationUpdate(connection, request, context, argumentsSerializer, onErrorCallback);
                case "delete":
                    return this.MutationDelete(connection, request, context, argumentsSerializer, onErrorCallback);
                default:
                    return Task.FromResult<JObject>(null);
            }
        }

        /// <summary>
        /// Creates filter expression by filter request
        /// </summary>
        /// <param name="filter">The filter request</param>
        /// <returns>The filter expression</returns>
        protected abstract Expression<Func<T, bool>> CreateFilter(JObject filter);

        /// <summary>
        /// Creates the list of sorting conditions by sorting request
        /// </summary>
        /// <param name="sortArguments">The sorting request</param>
        /// <returns>The list of sorting conditions</returns>
        protected abstract IEnumerable<SortingCondition> CreateSort(JArray sortArguments);

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
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
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Create(newNode);
            var mutationCreate = (JObject)await this.MutationResultResolver.ResolveQuery(result, request, context, argumentsSerializer, onErrorCallback);
            return mutationCreate;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
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
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedData = ((JObject)request.Arguments)?.Property("newNode")?.Value as JObject;
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
            var newNode = serializedData?.ToObject<T>();
            var result = await connection.Update(id, newNode, request);
            var mutationUpdate = (JObject)await this.MutationResultResolver.ResolveQuery(result, request, context, argumentsSerializer, onErrorCallback);
            if (result.Result != null)
            {
                if (id != null && !id.Equals(connection.GetId(result.Result)))
                {
                    mutationUpdate.Add("__deletedId", JsonConvert.SerializeObject(id));
                }
            }

            return mutationUpdate;
        }

        /// <summary>
        /// Runs the creation mutation
        /// </summary>
        /// <param name="connection">The connection</param>
        /// <param name="request">The request</param>
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
            RequestContext context,
            JsonSerializer argumentsSerializer,
            Action<Exception> onErrorCallback)
        {
            var serializedId = ((JObject)request.Arguments)?.Property("id")?.Value;
            var id = serializedId != null ? serializedId.ToObject<TId>() : default(TId);
            var result = await connection.Delete(id);
            var mutationDelete = (JObject)await this.MutationResultResolver.ResolveQuery(result, request, context, argumentsSerializer, onErrorCallback);
            if (result.Result != null)
            {
                mutationDelete.Add("__deletedId", JsonConvert.SerializeObject(id));
            }

            return mutationDelete;
        }
    }
}
