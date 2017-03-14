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
    public class Connection<TObject, TId> : INodeConnection<TObject, TId> where TObject : class, IObjectWithId<TId>, new()
    {
        /// <summary>
        /// The actor system
        /// </summary>
        private readonly ActorSystem actorSystem;

        /// <summary>
        /// The path to the data actor
        /// </summary>
        private readonly string dataActorPath;

        /// <summary>
        /// The request timeout
        /// </summary>
        private readonly TimeSpan? timeout;

        /// <summary>
        /// The request context
        /// </summary>
        private readonly RequestContext context;

        /// <inheritdoc />
        public Connection(ActorSystem actorSystem, string dataActorPath, TimeSpan? timeout, RequestContext context)
        {
            this.actorSystem = actorSystem;
            this.dataActorPath = dataActorPath;
            this.timeout = timeout;
            this.context = context;
        }

        /// <inheritdoc />
        public async Task<MutationResult<TObject>> Create(TObject newNode)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Create,
                                  Data = newNode,
                                  RequestContext = this.context
                              };

            var result =
                await this.actorSystem.ActorSelection(this.dataActorPath)
                    .Ask<CrudActionResponse<TObject>>(request, this.timeout);

            if (result.Exception != null)
            {
                var errorDescription = new ErrorDescription { Message = result.Exception.Message };
                var errorDescriptions = result.Exception != null ? new List<ErrorDescription> { errorDescription } : null;
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
                                  RequestContext = this.context
                              };

            var result =
                await this.actorSystem.ActorSelection(this.dataActorPath)
                    .Ask<CrudActionResponse<TObject>>(request, this.timeout);

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
        public async Task<TObject> GetById(TId id)
        {
            var request = new CrudActionMessage<TObject, TId>
            {
                ActionType = EnActionType.Get,
                Id = id,
                RequestContext = this.context
            };

            var result =
                await this.actorSystem.ActorSelection(this.dataActorPath)
                    .Ask<CrudActionResponse<TObject>>(request, this.timeout);

            return result.Data;
        }

        /// <inheritdoc />
        public async Task<QueryResult<TObject>> Query(
            Expression<Func<TObject, bool>> filter,
            IEnumerable<SortingCondition> sort,
            int? limit,
            int? offset,
            ApiRequest apiRequest)
        {
            this.actorSystem.Log.Info("{Type}: Query launched", this.GetType().Name);
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

            var parcel = await this.actorSystem.ActorSelection(this.dataActorPath).Ask<ParcelNotification>(request, this.timeout);
            var result = (CollectionResponse<TObject>)await parcel.Receive(this.actorSystem);
            return new QueryResult<TObject>
                       {
                           Count = result.Count,
                           Items = result.Items
                       };
        }

        /// <inheritdoc />
        public async Task<MutationResult<TObject>> Update(TId id, TObject newNode, ApiRequest apiRequest)
        {
            var request = new CrudActionMessage<TObject, TId>
            {
                ActionType = EnActionType.Update,
                Id = id,
                Data = newNode,
                RequestContext = this.context,
                ApiRequest = apiRequest
            };

            var result =
                await this.actorSystem.ActorSelection(this.dataActorPath)
                    .Ask<CrudActionResponse<TObject>>(request, this.timeout);

            if (result.Exception != null)
            {
                var errorDescription = new ErrorDescription { Message = result.Exception.Message };
                var errorDescriptions = result.Exception != null ? new List<ErrorDescription> { errorDescription } : null;
                return new MutationResult<TObject> { Result = result.Data, Errors = errorDescriptions };
            }

            return new MutationResult<TObject> { Result = result.Data };
        }

        /// <inheritdoc />
        public TId GetId(TObject node)
        {
            return node.GetId();
        }
    }
}
