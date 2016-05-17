// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic actor to perform basic crud operation on EF objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Data
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;

    using ClusterKit.Core.Rest.ActionMessages;

    using JetBrains.Annotations;

    /// <summary>
    /// Generic actor to perform basic crud operation on data objects
    /// </summary>
    /// <typeparam name="TContext">
    /// The database context
    /// </typeparam>
    public abstract class BaseCrudActor<TContext>
        : ReceiveActor
        where TContext : IDisposable
    {
        /// <summary>
        /// Method called after successful object creation in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="result">Created object</param>
        protected virtual void AfterCreate<TObject>(TObject result) where TObject : class
        {
        }

        /// <summary>
        /// Method called after successful object removal from database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="deletedObject">removed object</param>
        protected virtual void AfterDelete<TObject>(TObject deletedObject) where TObject : class
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
        protected virtual void AfterUpdate<TObject>(TObject newObject, TObject oldObject) where TObject : class
        {
        }

        /// <summary>
        /// Method call before object creation in database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="request">Object intended to be created</param>
        /// <returns>Object that will be created or null to prevent creation</returns>
        protected virtual TObject BeforeCreate<TObject>(TObject request) where TObject : class
        {
            return request;
        }

        /// <summary>
        /// Method call before object removal database
        /// </summary>
        /// <typeparam name="TObject">
        /// The type of ef object
        /// </typeparam>
        /// <param name="deletedObjects">Object intended to be removed</param>
        /// <returns>Object that will be removed</returns>
        protected virtual List<TObject> BeforeDelete<TObject>(List<TObject> deletedObjects) where TObject : class
        {
            return deletedObjects;
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
        protected virtual TObject BeforeUpdate<TObject>(TObject newObject, TObject oldObject) where TObject : class
        {
            return newObject;
        }

        /// <summary>
        /// Gets current data context
        /// </summary>
        /// <returns>The data context</returns>
        protected abstract Task<TContext> GetContext();

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
            using (var ds = await this.GetContext())
            {
                var factory = DataFactory<TContext, TObject, TId>.CreateFactory(ds);
                this.Sender.Tell(await factory.GetList(collectionRequest.Skip, collectionRequest.Count));
            }
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
        protected virtual async Task OnRequest<TObject, TId>(RestActionMessage<TObject, TId> request)
            where TObject : class
        {
            using (var ds = await this.GetContext())
            {
                var factory = DataFactory<TContext, TObject, TId>.CreateFactory(ds);
                switch (request.ActionType)
                {
                    case EnActionType.Get:
                        this.Sender.Tell(this.OnSelect(await factory.Get(request.Id)));
                        break;

                    case EnActionType.Create:
                        {
                            var entity = request.Data;
                            if (entity == null)
                            {
                                // ReSharper disable FormatStringProblem
                                Context.GetLogger().Error("{Type}: create failed, no data", this.GetType().Name);
                                // ReSharper restore FormatStringProblem
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            var oldObject = await factory.Get(factory.GetId(entity));
                            if (oldObject != null)
                            {
                                Context.GetLogger()
                                    .Error(
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: create failed, there is already object with id {Id}",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        factory.GetId(entity).ToString());
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            entity = this.BeforeCreate(entity);

                            if (entity == null)
                            {
                                Context.GetLogger()
                                    // ReSharper disable FormatStringProblem
                                    .Error("{Type}: create failed, prevented by BeforeCreate", this.GetType().Name);
                                // ReSharper restore FormatStringProblem
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            try
                            {
                                await factory.Insert(entity);
                                this.Sender.Tell(new RestActionResponse<TObject>(entity, request.ExtraData));
                                this.AfterCreate(entity);
                                return;
                            }
                            catch (Exception exception)
                            {
                                Context.GetLogger()
                                    .Error(
                                        exception,
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: create failed, error while creating object with id {Id}",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        factory.GetId(entity).ToString());
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }
                        }

                    case EnActionType.Update:
                        {
                            var entity = request.Data;
                            if (entity == null)
                            {
                                // ReSharper disable FormatStringProblem
                                Context.GetLogger().Error("{Type}: create failed, no data", this.GetType().Name);
                                // ReSharper restore FormatStringProblem
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            var oldObject = await factory.Get(request.Id);
                            if (oldObject == null)
                            {
                                Context.GetLogger()
                                    .Error(
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: update failed, there is no object with id {Id}",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        // ReSharper disable RedundantToStringCallForValueType
                                        request.Id.ToString());
                                // ReSharper restore RedundantToStringCallForValueType
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            entity = this.BeforeUpdate<TObject>(entity, oldObject);
                            if (entity == null)
                            {
                                Context.GetLogger()
                                    .Error(
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: update of object with id {Id} failed, prevented by BeforeUpdate",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        // ReSharper disable RedundantToStringCallForValueType
                                       request.Id.ToString());
                                // ReSharper restore RedundantToStringCallForValueType
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }

                            try
                            {
                                await factory.Update(entity, oldObject);
                                this.Sender.Tell(new RestActionResponse<TObject>(entity, request.ExtraData));
                                this.AfterUpdate<TObject>(entity, oldObject);
                                return;
                            }
                            catch (Exception exception)
                            {
                                Context.GetLogger()
                                    .Error(
                                        exception,
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: update failed, error while creating object with id {Id}",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        // ReSharper disable RedundantToStringCallForValueType
                                        factory.GetId(entity).ToString());
                                // ReSharper restore RedundantToStringCallForValueType
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }
                        }

                    case EnActionType.Delete:
                        {
                            try
                            {
                                var oldObject = await factory.Delete(request.Id);
                                if (oldObject == null)
                                {
                                    Context.GetLogger()
                                        .Error(
                                            // ReSharper disable FormatStringProblem
                                            "{Type}: delete failed, there is no object with id {Id}",
                                            // ReSharper restore FormatStringProblem
                                            this.GetType().Name,
                                            // ReSharper disable RedundantToStringCallForValueType
                                            request.Id.ToString());
                                    // ReSharper restore RedundantToStringCallForValueType
                                    this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                }

                                this.AfterDelete<TObject>(oldObject);
                                this.Sender.Tell(new RestActionResponse<TObject>(oldObject, request.ExtraData));
                                return;
                            }
                            catch (Exception exception)
                            {
                                Context.GetLogger()
                                    .Error(
                                        exception,
                                        // ReSharper disable FormatStringProblem
                                        "{Type}: delete failed, error while deleting object with id {Id}",
                                        // ReSharper restore FormatStringProblem
                                        this.GetType().Name,
                                        // ReSharper disable RedundantToStringCallForValueType
                                        request.Id.ToString());
                                // ReSharper restore RedundantToStringCallForValueType
                                this.Sender.Tell(new RestActionResponse<TObject>(null, request.ExtraData));
                                return;
                            }
                        }

                    default:
                        throw new ArgumentOutOfRangeException();
                }
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
    }
}