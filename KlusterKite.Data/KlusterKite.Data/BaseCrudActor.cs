﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudActor.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Generic actor to perform basic crud operation on EF objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Reflection;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;

    using Autofac;

    using JetBrains.Annotations;

    using KlusterKite.API.Attributes;
    using KlusterKite.API.Client;
    using KlusterKite.Data.CRUD;
    using KlusterKite.Data.CRUD.ActionMessages;
    using KlusterKite.Data.CRUD.Exceptions;
    using KlusterKite.LargeObjects;
    using KlusterKite.LargeObjects.Client;
    using KlusterKite.Security.Attributes;
    using KlusterKite.Security.Client;

    /// <summary>
    /// Generic actor to perform basic crud operation on data objects
    /// </summary>
    /// <typeparam name="TContext">
    /// The database context
    /// </typeparam>
    public abstract class BaseCrudActor<TContext> : ReceiveActor
        where TContext : IDisposable
    {
        /// <summary>
        /// The list of individual class limits
        /// </summary>
        private readonly IReadOnlyDictionary<string, int> classQueryLimits;

        /// <summary>
        /// The default query limit
        /// </summary>
        private readonly int defaultQueryLimit;

        /// <inheritdoc />
        protected BaseCrudActor(IComponentContext componentContext)
        {
            this.ComponentContext = componentContext;
            this.Receive<ParcelException>(m => this.OnParcelException(m));

            var configSection = Context.System.Settings.Config.GetConfig("KlusterKite.Data.Crud.Query.Limits");
            this.defaultQueryLimit = configSection?.GetInt("Default", 100) ?? 100;
            var classLimits = new Dictionary<string, int>();
            if (configSection != null)
            {
                foreach (var valuePair in configSection.AsEnumerable())
                {
                    classLimits[valuePair.Key] = valuePair.Value.GetInt();
                }
            }

            this.classQueryLimits = classLimits.ToImmutableDictionary();
        }

        /// <summary>
        /// Gets the DI component context
        /// </summary>
        protected IComponentContext ComponentContext { get; }

        /// <summary>
        /// Method called after successful object creation in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="result">Created object</param>
        protected virtual void AfterCreate<TObject>(TObject result)
            where TObject : class
        {
        }

        /// <summary>
        /// Method called after successful object removal from database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="deletedObject">removed object</param>
        protected virtual void AfterDelete<TObject>(TObject deletedObject)
            where TObject : class
        {
        }

        /// <summary>
        /// Method called after successful object modification in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="newObject">
        /// The new Object.
        /// </param>
        /// <param name="oldObject">
        /// The old Object.
        /// </param>
        protected virtual void AfterUpdate<TObject>(TObject newObject, TObject oldObject)
            where TObject : class
        {
        }

        /// <inheritdoc />
        protected override bool AroundReceive(Receive receive, object message)
        {
            var parcel = message as ParcelNotification;

            if (parcel != null)
            {
                parcel.ReceiveWithPipeTo(ActorBase.Context.System, this.Self, this.Sender);
                return true;
            }

            return base.AroundReceive(receive, message);
        }

        /// <summary>
        /// Method call before object creation in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="request">Object intended to be created</param>
        /// <returns>Object that will be created or null to prevent creation</returns>
        protected virtual TObject BeforeCreate<TObject>(TObject request)
            where TObject : class
        {
            return request;
        }

        /// <summary>
        /// Method call before object removal database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="oldObject">Object intended to be removed</param>
        /// <returns>Object that will be removed</returns>
        protected virtual TObject BeforeDelete<TObject>(TObject oldObject)
            where TObject : class
        {
            return oldObject;
        }

        /// <summary>
        /// Method called before object modification in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="newObject">
        /// The new Object.
        /// </param>
        /// <param name="oldObject">
        /// The old Object.
        /// </param>
        /// <returns>
        /// The new version of object or null to prevent update
        /// </returns>
        protected virtual TObject BeforeUpdate<TObject>(TObject newObject, TObject oldObject)
            where TObject : class
        {
            return newObject;
        }

        /// <summary>
        /// Gets current data context
        /// </summary>
        /// <returns>The data context</returns>
        protected abstract TContext GetContext();

        /// <summary>
        /// Process collection requests
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <typeparam name="TId">
        /// The type of object identity field
        /// </typeparam>
        /// <param name="collectionRequest">Collection request</param>
        /// <returns>The list of objects</returns>
        protected virtual async Task OnCollectionRequest<TObject, TId>(CollectionRequest<TObject> collectionRequest)
            where TObject : class
        {
            try
            {
                using (var ds = this.GetContext())
                {
                    int maxLimit;
                    if (!this.classQueryLimits.TryGetValue(
                            NamingUtilities.ToCSharpRepresentation(typeof(TObject)),
                            out maxLimit))
                    {
                        maxLimit = this.defaultQueryLimit;
                    }

                    var limit = maxLimit != 0 && (!collectionRequest.Count.HasValue
                                                  || collectionRequest.Count.Value > maxLimit)
                                    ? maxLimit
                                    : collectionRequest.Count;

                    var factory = DataFactory<TContext, TObject, TId>.CreateFactory(this.ComponentContext, ds);

                    var response = await factory.GetList(
                                       collectionRequest.Filter,
                                       collectionRequest.Sort,
                                       collectionRequest.Skip,
                                       limit,
                                       collectionRequest.ApiRequest);

                    if (collectionRequest.AcceptAsParcel)
                    {
                        Context.GetParcelManager().Tell(
                            new Parcel { Payload = response, Recipient = this.Sender },
                            this.Self);
                    }
                    else
                    {
                        this.Sender.Tell(response);
                    }
                }
            }
            catch (Exception exception)
            {
                try
                {
                    Context.GetLogger().Error(
                        exception,
                        "{Type}: Exception on processing CollectionRequest\n\t filter: {FilterExpression}\n\t sort: {SortExpression}\n\t limit: {Limit}\n\t offset: {Offset}",
                        $"BaseCrudActor<{typeof(TObject).Name}>",
                        collectionRequest.Filter?.ToString(),
                        collectionRequest.Sort?.ToString(),
                        collectionRequest.Count,
                        collectionRequest.Skip);
                }
                catch
                {
                    Context.GetLogger().Error(
                        exception,
                        "{Type}: Exception on processing CollectionRequest",
                        $"BaseCrudActor<{typeof(TObject).Name}>");
                }

                var response = new CollectionResponse<TObject> { Items = new List<TObject>() };
                if (collectionRequest.AcceptAsParcel)
                {
                    Context.GetParcelManager().Tell(
                        new Parcel { Payload = response, Recipient = this.Sender },
                        this.Self);
                }
                else
                {
                    this.Sender.Tell(response);
                }
            }
        }

        /// <summary>
        /// Handling the parcel receive exceptions
        /// </summary>
        /// <param name="message">The parcel receive exception</param>
        protected virtual void OnParcelException(ParcelException message)
        {
            var errorMessage = message.Message;
            var logTemplate = string.IsNullOrWhiteSpace(errorMessage)
                                  ? "{Type}: Parcel receive error"
                                  : "{Type}: Parcel receive error: {ErrorMessage}";

            Context.GetLogger().Error(message, logTemplate, this.GetType().Name, errorMessage);
        }

        /// <summary>
        /// Request process method
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <typeparam name="TId">
        /// The type of object identity field
        /// </typeparam>
        /// <param name="request">
        /// The action request
        /// </param>
        /// <returns>
        /// Execution task
        /// </returns>
        [UsedImplicitly]
        protected virtual async Task OnRequest<TObject, TId>(CrudActionMessage<TObject, TId> request)
            where TObject : class
        {
            CrudActionResponse<TObject> response;
            try
            {
                response = await this.ProcessRequest(request);
            }
            catch (Exception exception)
            {
                Context.GetLogger().Error(
                    exception,
                    "{Type}: Error while processing {ActionType} for {EntityType}",
                    this.GetType().Name,
                    request.ActionType,
                    typeof(TObject).Name);
                response = new CrudActionResponse<TObject> { Exception = exception, ExtraData = request.ExtraData };
            }

            if (typeof(ILargeObject).GetTypeInfo().IsAssignableFrom(typeof(TObject)))
            {
                Context.GetParcelManager().Tell(new Parcel { Payload = response, Recipient = this.Sender }, this.Self);
            }
            else
            {
                this.Sender.Tell(response);
            }
        }

        /// <summary>
        /// Called on select. Sender will receive this method output.
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="result">Selected object from database</param>
        /// <returns>Result for requester</returns>
        protected virtual TObject OnSelect<TObject>(TObject result)
        {
            return result;
        }

        /// <summary>
        /// Request process method
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <typeparam name="TId">
        /// The type of object identity field
        /// </typeparam>
        /// <param name="request">
        /// The action request
        /// </param>
        /// <returns>
        /// Execution task
        /// </returns>
        [UsedImplicitly]
        protected virtual async Task<CrudActionResponse<TObject>> ProcessRequest<TObject, TId>(
            CrudActionMessage<TObject, TId> request)
            where TObject : class
        {
            using (var ds = this.GetContext())
            {
                var factory = DataFactory<TContext, TObject, TId>.CreateFactory(this.ComponentContext, ds);
                switch (request.ActionType)
                {
                    case EnActionType.Get:
                        try
                        {
                            var result = await factory.Get(request.Id);
                            if (result.HasValue)
                            {
                                result = this.OnSelect(result.Value);
                            }

                            // security read log should be set on client endpoint
                            return result.HasValue
                                       ? CrudActionResponse<TObject>.Success(result, request.ExtraData)
                                       : CrudActionResponse<TObject>.Error(
                                           new EntityNotFoundException(),
                                           request.ExtraData);
                        }
                        catch (Exception exception)
                        {
                            return CrudActionResponse<TObject>.Error(
                                new DatasourceInnerException("Exception on Get operation", exception),
                                request.ExtraData);
                        }

                    case EnActionType.Create:
                        {
                            var entity = request.Data;
                            if (entity == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new RequestEmptyException(),
                                    request.ExtraData);
                            }

                            entity = this.BeforeCreate(entity);
                            var oldObject = await factory.Get(factory.GetId(entity));
                            if (oldObject != null)
                            {
                                var crudActionResponse = CrudActionResponse<TObject>.Error(
                                    new InsertDuplicateIdException(),
                                    request.ExtraData);
                                crudActionResponse.Data = oldObject;

                                return crudActionResponse;
                            }

                            if (entity == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new BeforeActionException(),
                                    request.ExtraData);
                            }

                            try
                            {
                                await factory.Insert(entity);

                                // security update logs are set here to be sure that they are made independently of client notification success
                                SecurityLog.CreateRecord(
                                    EnSecurityLogType.DataCreateGranted,
                                    entity is ICrucialObject ? EnSeverity.Crucial : EnSeverity.Trivial,
                                    request.RequestContext,
                                    "{ObjectType} with {ObjectId} id was created",
                                    typeof(TObject).FullName,
                                    factory.GetId(entity));

                                this.AfterCreate(entity);
                                return CrudActionResponse<TObject>.Success(entity, request.ExtraData);
                            }
                            catch (Exception exception)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new DatasourceInnerException("Exception on Insert operation", exception),
                                    request.ExtraData);
                            }
                        }

                    case EnActionType.Update:
                        {
                            var entity = request.Data;
                            if (entity == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new RequestEmptyException(),
                                    request.ExtraData);
                            }

                            var oldObject = await factory.Get(request.Id);
                            if (oldObject == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new EntityNotFoundException(),
                                    request.ExtraData);
                            }

                            if (request.ApiRequest != null)
                            {
                                var updatedObject = await factory.Get(request.Id);
                                if (updatedObject == null)
                                {
                                    return CrudActionResponse<TObject>.Error(
                                        new EntityNotFoundException(),
                                        request.ExtraData);
                                }

                                DataUpdater<TObject>.Update(updatedObject, entity, request.ApiRequest);
                                entity = updatedObject;
                            }

                            entity = this.BeforeUpdate<TObject>(entity, oldObject);
                            if (entity == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new BeforeActionException(),
                                    request.ExtraData);
                            }

                            try
                            {
                                await factory.Update(entity, oldObject);

                                // security update logs are set here to be sure that they are made independently of client notification success
                                if (!factory.GetId(entity).Equals(factory.GetId(oldObject)))
                                {
                                    SecurityLog.CreateRecord(
                                        EnSecurityLogType.DataUpdateGranted,
                                        entity is ICrucialObject ? EnSeverity.Crucial : EnSeverity.Trivial,
                                        request.RequestContext,
                                        "{ObjectType} with id {ObjectId} was updated. New id is {NewObjectId}",
                                        typeof(TObject).FullName,
                                        factory.GetId(oldObject),
                                        factory.GetId(entity));
                                }
                                else
                                {
                                    SecurityLog.CreateRecord(
                                        EnSecurityLogType.DataUpdateGranted,
                                        entity is ICrucialObject ? EnSeverity.Crucial : EnSeverity.Trivial,
                                        request.RequestContext,
                                        "{ObjectType} with id {ObjectId} was updated.",
                                        typeof(TObject).FullName,
                                        factory.GetId(entity));
                                }

                                this.AfterUpdate<TObject>(entity, oldObject);
                                return CrudActionResponse<TObject>.Success(entity, request.ExtraData);
                            }
                            catch (Exception exception)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new DatasourceInnerException("Exception on Update operation", exception),
                                    request.ExtraData);
                            }
                        }

                    case EnActionType.Delete:
                        {
                            var oldObject = await factory.Get(request.Id);
                            if (oldObject == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new EntityNotFoundException(),
                                    request.ExtraData);
                            }

                            oldObject = this.BeforeDelete(oldObject.Value);
                            if (oldObject == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new BeforeActionException(),
                                    request.ExtraData);
                            }

                            try
                            {
                                oldObject = await factory.Delete(factory.GetId(oldObject));
                            }
                            catch (Exception exception)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new DatasourceInnerException("Exception on Delete operation", exception),
                                    request.ExtraData);
                            }

                            if (oldObject == null)
                            {
                                return CrudActionResponse<TObject>.Error(
                                    new EntityNotFoundException("After \"Before\" action modification"),
                                    request.ExtraData);
                            }

                            // security update logs are set here to be sure that they are made independently of client notification success
                            SecurityLog.CreateRecord(
                                EnSecurityLogType.DataDeleteGranted,
                                oldObject.Value is ICrucialObject ? EnSeverity.Crucial : EnSeverity.Trivial,
                                request.RequestContext,
                                "{ObjectType} with id {ObjectId} was deleted.",
                                typeof(TObject).FullName,
                                factory.GetId(oldObject));

                            this.AfterDelete<TObject>(oldObject);
                            return CrudActionResponse<TObject>.Success(oldObject, request.ExtraData);
                        }

                    default:
                        return CrudActionResponse<TObject>.Error(
                            new ArgumentOutOfRangeException(nameof(request.ActionType)),
                            request.ExtraData);
                }
            }
        }
    }
}