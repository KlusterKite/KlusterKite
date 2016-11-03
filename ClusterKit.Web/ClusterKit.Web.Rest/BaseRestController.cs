﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Akka.Actor;

    using ClusterKit.Core;
    using ClusterKit.Data.CRUD.ActionMessages;
    using ClusterKit.Data.CRUD.Exceptions;
    using ClusterKit.LargeObjects;
    using ClusterKit.LargeObjects.Client;

    using JetBrains.Annotations;

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
    public abstract class BaseRestController<TObject, TId> : ApiController where TObject : class
    {
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
        /// Gets timeout for actor system requests
        /// </summary>
        protected virtual TimeSpan AkkaTimeout { get; }

        /// <summary>
        /// The data is large.
        /// </summary>
        /// <remarks>
        /// Large data will be sent and received via parcels pipe. This will have impact on performance, but does not have message size limitations
        /// </remarks>
        protected virtual bool DataIsLarge => false;

        /// <summary>
        /// The list of returned objects is large.
        /// </summary>
        /// <remarks>
        /// Large data will be sent and received via parcels pipe. This will have impact on performance, but does not have message size limitations
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
        /// <returns>The new object, as it was created in datasource</returns>
        [HttpPut]
        [Route("")]
        [UsedImplicitly]
        public virtual async Task<TObject> Create(TObject data)
        {
            var request = new CrudActionMessage<TObject, TId> { ActionType = EnActionType.Create, Data = data };
            return await this.SendRequest(request);
        }

        /// <summary>
        /// Removes object
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <returns>The removed object</returns>
        [HttpDelete]
        [Route("{id}/")]
        [UsedImplicitly]
        public virtual async Task<TObject> Delete(TId id)
        {
            var request = new CrudActionMessage<TObject, TId> { ActionType = EnActionType.Delete, Id = id };
            return await this.SendRequest(request);
        }

        /// <summary>
        /// Gets an object by it's id
        /// </summary>
        /// <param name="id">The object's unique id</param>
        /// <returns>The object</returns>
        [HttpGet]
        [Route("{id}/")]
        [UsedImplicitly]
        public virtual async Task<TObject> Get(TId id)
        {
            var request = new CrudActionMessage<TObject, TId> { ActionType = EnActionType.Get, Id = id };
            return await this.SendRequest(request);
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
        public virtual async Task<List<TObject>> GetList(int count = 100, int skip = 0)
        {
            var collectionRequest = new CollectionRequest<TObject> { Count = count, Skip = skip };

            if (this.DataListIsLarge || this.DataIsLarge)
            {
                collectionRequest.AcceptAsParcel = true;
                var notification =
                    await
                        this.System.ActorSelection(this.GetDbActorProxyPath())
                            .Ask<ParcelNotification>(collectionRequest, this.AkkaTimeout);

                return (List<TObject>)await notification.Receive(this.System);
            }

            collectionRequest.AcceptAsParcel = false;
            return
                await
                    this.System.ActorSelection(this.GetDbActorProxyPath())
                        .Ask<List<TObject>>(collectionRequest, this.AkkaTimeout);
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
        public virtual async Task<TObject> Update([FromUri] TId id, [FromBody] TObject data)
        {
            var request = new CrudActionMessage<TObject, TId> { ActionType = EnActionType.Update, Data = data, Id = id };
            return await this.SendRequest(request);
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
            if (this.DataIsLarge)
            {
                var notification =
                    await
                        this.System.GetParcelManager()
                            .Ask<ParcelNotification>(
                                new Parcel
                                {
                                    Payload = request,
                                    Recipient = this.System.ActorSelection(this.GetDbActorProxyPath())
                                },
                                this.AkkaTimeout);

                result = (CrudActionResponse<TObject>)await notification.Receive(this.System);
            }
            else
            {
                result =
                    await
                        this.System.ActorSelection(this.GetDbActorProxyPath())
                            .Ask<CrudActionResponse<TObject>>(request, this.AkkaTimeout);
            }

            if (result.Exception != null && result.Exception.GetType() != typeof(EntityNotFoundException))
            {
                throw result.Exception;
            }

            if (result.Exception?.GetType() == typeof(EntityNotFoundException) || result.Data == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }

            return result.Data;
        }
    }
}