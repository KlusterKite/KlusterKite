// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Connection.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   The base connection using data actors
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.CRUD
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.LargeObjects.Client;
    using ClusterKit.Security.Client;

    /// <summary>
    /// The base connection using data actors
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of entity
    /// </typeparam>
    /// <typeparam name="TId">
    /// The type of object id
    /// </typeparam>
    /// TODO: remove TId type parameter and recover it from type data
    public class Connection<TObject, TId> : INodeConnection<TObject>
        where TObject : class, IObjectWithId<TId>, new()
    {
        /// <inheritdoc />
        public Connection(ActorSystem actorSystem, string dataActorPath, TimeSpan? timeout, RequestContext context)
        {
            this.System = actorSystem;
            this.DataActorPath = dataActorPath;
            this.Timeout = timeout;
            this.Context = context;
        }

        /// <summary>
        /// Gets the request context
        /// </summary>
        protected RequestContext Context { get; }

        /// <summary>
        /// Gets the path to the data actor
        /// </summary>
        protected string DataActorPath { get; }

        /// <summary>
        /// Gets the actor system
        /// </summary>
        protected ActorSystem System { get; }

        /// <summary>
        /// Gets the request timeout
        /// </summary>
        protected TimeSpan? Timeout { get; }

        /// <inheritdoc />
        public async Task<MutationResult<TObject>> Create(TObject newNode)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Create,
                                  Data = newNode,
                                  RequestContext = this.Context
                              };

            var result = await this.System.ActorSelection(this.DataActorPath)
                             .Ask<CrudActionResponse<TObject>>(request, this.Timeout);

            if (result.Exception != null)
            {
                var errorDescription = new ErrorDescription { Message = result.Exception.Message };
                var errorDescriptions = result.Exception != null
                                            ? new List<ErrorDescription> { errorDescription }
                                            : null;
                return new MutationResult<TObject> { Result = result.Data, Errors = errorDescriptions };
            }

            return new MutationResult<TObject> { Result = result.Data };
        }

        /// <inheritdoc />
        public async Task<MutationResult<TObject>> Delete(TId id)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Delete,
                                  Id = id,
                                  RequestContext = this.Context
                              };

            var result = await this.System.ActorSelection(this.DataActorPath)
                             .Ask<CrudActionResponse<TObject>>(request, this.Timeout);

            if (result.Exception != null)
            {
                var errorDescription = new ErrorDescription { Message = result.Exception.Message };
                var errorDescriptions = result.Exception != null
                                            ? new List<ErrorDescription> { errorDescription }
                                            : null;
                return new MutationResult<TObject> { Result = result.Data, Errors = errorDescriptions };
            }

            return new MutationResult<TObject> { Result = result.Data };
        }

        /// <inheritdoc />
        public TId GetId(TObject node)
        {
            return node.GetId();
        }

        /// <inheritdoc />
        public async Task<QueryResult<TObject>> Query(
            Expression<Func<TObject, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest)
        {
            var sortingConditions = sort?.ToList() ?? new List<SortingCondition>();

            if (sortingConditions.Count == 0)
            {
                offset = null;
            }

            ApiRequest combinedRequest = null;
            if (apiRequest != null)
            {
                combinedRequest = new ApiRequest
                                      {
                                          Fields =
                                              apiRequest.Fields.Where(f => f.FieldName == "items")
                                                  .SelectMany(f => f.Fields)
                                                  .ToList()
                                      };
            }

            var request = new CollectionRequest<TObject>
                              {
                                  Count = limit,
                                  Skip = offset,
                                  Filter = filter,
                                  Sort = sortingConditions,
                                  AcceptAsParcel = true,
                                  ApiRequest = combinedRequest
                              };

            var parcel = await this.System.ActorSelection(this.DataActorPath)
                             .Ask<ParcelNotification>(request, this.Timeout);
            var result = (CollectionResponse<TObject>)await parcel.Receive(this.System);
            return new QueryResult<TObject> { Count = result.Count, Items = result.Items };
        }

        /// <inheritdoc />
        public async Task<MutationResult<TObject>> Update(TId id, TObject newNode, ApiRequest apiRequest)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Update,
                                  Id = id,
                                  Data = newNode,
                                  RequestContext = this.Context,
                                  ApiRequest = apiRequest
                              };

            var result = await this.System.ActorSelection(this.DataActorPath)
                             .Ask<CrudActionResponse<TObject>>(request, this.Timeout);

            if (result.Exception != null)
            {
                var errorDescription = new ErrorDescription { Message = result.Exception.Message };
                var errorDescriptions = result.Exception != null
                                            ? new List<ErrorDescription> { errorDescription }
                                            : null;
                return new MutationResult<TObject> { Result = result.Data, Errors = errorDescriptions };
            }

            return new MutationResult<TObject> { Result = result.Data };
        }

        /// <inheritdoc />
        Task<MutationResult<TObject>> INodeConnection<TObject>.Delete(object id)
        {
            return this.Delete((TId)id);
        }

        /// <inheritdoc />
        Task<MutationResult<TObject>> INodeConnection<TObject>.Update(object id, TObject newNode, ApiRequest request)
        {
            return this.Update((TId)id, newNode, request);
        }
    }
}