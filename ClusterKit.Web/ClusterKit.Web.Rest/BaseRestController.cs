// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseRestController.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base class to provide basic CRUD operation for REST service
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Web.Rest
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;

    using Akka.Actor;

    using ClusterKit.API.Client;
    using ClusterKit.Core;
    using ClusterKit.Data.CRUD;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.CRUD.Exceptions;
    using ClusterKit.LargeObjects;
    using ClusterKit.LargeObjects.Client;
    using ClusterKit.Security.Attributes;
    using ClusterKit.Security.Client;
    using ClusterKit.Web.Authorization;

    using JetBrains.Annotations;

    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// Base class to provide basic CRUD operation for REST service
    /// </summary>
    /// <typeparam name="TObject">
    /// The type of ef object
    /// </typeparam>
    /// <typeparam name="TId">
    /// The type of object identity field
    /// </typeparam>
    [UsedImplicitly]
    public abstract class BaseRestController<TObject, TId> : Controller
        where TObject : class
    {
        /// <summary>
        /// Initializes static members of the <see cref="BaseRestController{TObject,TId}"/> class.
        /// </summary>
        static BaseRestController()
        {
            DataIsLarge = typeof(ILargeObject).IsAssignableFrom(typeof(TObject));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseRestController{TObject,TId}"/> class.
        /// </summary>
        /// <param name="system">
        /// The system.
        /// </param>
        protected BaseRestController(ActorSystem system)
        {
            this.System = system;
            this.AkkaTimeout = ConfigurationUtils.GetRestTimeout(system);
        }

        /// <summary>
        /// Gets a value indicating whether the individual data stored in entity is large enough to be send via parcels.
        /// </summary>
        [UsedImplicitly]
        // ReSharper disable once StaticMemberInGenericType
        protected static bool DataIsLarge { get; private set; }

        /// <summary>
        /// Gets the filtering condition
        /// </summary>
        protected abstract Expression<Func<TObject, bool>> DefaultFilter { get; }

        /// <summary>
        /// Gets the sorting function
        /// </summary>
        protected abstract List<SortingCondition> DefaultSort { get; }

        /// <summary>
        /// Gets timeout for actor system requests
        /// </summary>
        protected virtual TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// The list of returned objects is large.
        /// </summary>
        /// <remarks>
        /// Large data will be sent and received via parcels pipe. This will have impact on performance, but does not have message size limitations.
        /// In case of <see cref="DataIsLarge"/> this property will be ignored
        /// </remarks>
        protected virtual bool DataListIsLarge => true;

        /// <summary>
        /// Gets the actor system
        /// </summary>
        [UsedImplicitly]
        protected ActorSystem System { get; }

        /// <summary>
        /// Creates the object
        /// </summary>
        /// <param name="data">The object data</param>
        /// <returns>The new object, as it was created in data source</returns>
        [Microsoft.AspNetCore.Mvc.HttpPut]
        [Microsoft.AspNetCore.Mvc.Route("")]
        [UsedImplicitly]
        public virtual async Task<IActionResult> Create(TObject data)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Create,
                                  Data = data,
                                  RequestContext =
                                      this.GetRequestDescription()
                              };
            var result = await this.SendRequest(request);
            return this.Ok(result);
        }

        /// <summary>
        /// Removes object
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <returns>The removed object</returns>
        [HttpDelete]
        [Route("{id}/")]
        [UsedImplicitly]
        public virtual async Task<IActionResult> Delete(TId id)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Delete,
                                  Id = id,
                                  RequestContext =
                                      this.GetRequestDescription()
                              };
            return this.Ok(await this.SendRequest(request));
        }

        /// <summary>
        /// Gets an object by it's id
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <returns>The object</returns>
        [HttpGet]
        [Route("{id}/")]
        [UsedImplicitly]
        public virtual async Task<IActionResult> Get(TId id)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Get,
                                  Id = id,
                                  RequestContext =
                                      this.GetRequestDescription()
                              };
            var response = await this.SendRequest(request);

            SecurityLog.CreateRecord(
                EnSecurityLogType.DataReadGranted,
                response is ICrucialObject ? EnSeverity.Crucial : EnSeverity.Trivial,
                this.GetRequestDescription(),
                "{ObjectType} with id {ObjectId} was read.",
                typeof(TObject).FullName,
                id);

            return this.Ok(response);
        }

        /// <summary>
        /// Gets the list of all node templates
        /// </summary>
        /// <param name="count">
        /// The count of elements to return.
        /// </param>
        /// <param name="skip">
        /// The count of elements to skip.
        /// </param>
        /// <returns>
        /// list of node templates
        /// </returns>
        [Route("")]
        [HttpGet]
        [UsedImplicitly]
        public virtual async Task<IActionResult> GetList(int count = 100, int skip = 0)
        {
            var collectionRequest = new CollectionRequest<TObject>
                                        {
                                            Filter = this.DefaultFilter,
                                            Sort = this.DefaultSort,
                                            Count = count,
                                            Skip = skip,
                                            RequestContext =
                                                this.GetRequestDescription()
                                        };

            CollectionResponse<TObject> response;
            if (this.DataListIsLarge || DataIsLarge)
            {
                collectionRequest.AcceptAsParcel = true;
                var notification =
                    await this.System.ActorSelection(this.GetDbActorProxyPath())
                        .Ask<ParcelNotification>(collectionRequest, this.AkkaTimeout);
                response = (CollectionResponse<TObject>)await notification.Receive(this.System);
            }
            else
            {
                collectionRequest.AcceptAsParcel = false;
                response =
                    await this.System.ActorSelection(this.GetDbActorProxyPath())
                        .Ask<CollectionResponse<TObject>>(collectionRequest, this.AkkaTimeout);
            }

            var severity = typeof(TObject).GetInterfaces().Any(i => i == typeof(ICrucialObject))
                                 ? EnSeverity.Crucial
                                 : EnSeverity.Trivial;

            SecurityLog.CreateRecord(
                EnSecurityLogType.DataReadGranted,
                severity,
                this.GetRequestDescription(),
                "The list of {ObjectType} was read.",
                typeof(TObject).FullName);

            return this.Ok(response);
        }

        /// <summary>
        /// Updates the object data
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <param name="data">The object data</param>
        /// <returns>
        /// The updated object
        /// </returns>
        [HttpPatch]
        [Route("{id}/")]
        [UsedImplicitly]
        public virtual async Task<IActionResult> Update(TId id, [FromBody] TObject data)
        {
            var request = new CrudActionMessage<TObject, TId>
                              {
                                  ActionType = EnActionType.Update,
                                  Data = data,
                                  Id = id,
                                  RequestContext =
                                      this.GetRequestDescription()
                              };
            return this.Ok(await this.SendRequest(request));
        }

        /// <summary>
        /// Gets akka actor path for database worker
        /// </summary>
        /// <returns>Akka actor path</returns>
        protected abstract string GetDbActorProxyPath();

        /// <summary>
        /// Creates request to actor system and accepts response
        /// </summary>
        /// <param name="request">The request to the actor system</param>
        /// <returns>The object</returns>
        protected virtual async Task<TObject> SendRequest(CrudActionMessage<TObject, TId> request)
        {
            CrudActionResponse<TObject> result;
            if (DataIsLarge)
            {
                var notification = request.ActionType == EnActionType.Get
                                       ? await this.System.ActorSelection(this.GetDbActorProxyPath())
                                             .Ask<ParcelNotification>(request, this.AkkaTimeout)
                                       : await this.System.GetParcelManager()
                                             .Ask<ParcelNotification>(
                                                 new Parcel
                                                     {
                                                         Payload = request,
                                                         Recipient =
                                                             this.System.ActorSelection(
                                                                 this.GetDbActorProxyPath())
                                                     },
                                                 this.AkkaTimeout);

                result = (CrudActionResponse<TObject>)await notification.Receive(this.System);
            }
            else
            {
                result =
                    await this.System.ActorSelection(this.GetDbActorProxyPath())
                        .Ask<CrudActionResponse<TObject>>(request, this.AkkaTimeout);
            }

            if (result.Exception != null && result.Exception.GetType() != typeof(EntityNotFoundException))
            {
                throw result.Exception;
            }

            if (result.Exception?.GetType() == typeof(EntityNotFoundException) || result.Data == null)
            {
                return null;
            }

            return result.Data;
        }
    }
}