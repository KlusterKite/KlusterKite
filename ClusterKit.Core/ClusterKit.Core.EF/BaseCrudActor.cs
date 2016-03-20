// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic actor to permom basic crud operation on EF objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

// ReSharper disable FormatStringProblem
namespace ClusterKit.Core.EF
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Akka.Actor;
    using Akka.Event;

    using ClusterKit.Core.Rest.ActionMessages;

    /// <summary>
    /// Generic actor to perform basic crud operation on EF objects
    /// </summary>
    /// <typeparam name="TContext">
    /// The database context
    /// </typeparam>
    public abstract class BaseCrudActor<TContext>
        : ReceiveActor
        where TContext : DbContext
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
        /// <param name="deletedObjects">List of removed objects</param>
        protected virtual void AfterDelete<TObject>(List<TObject> deletedObjects) where TObject : class
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
        /// <param name="collectionRequest">Collection request</param>
        /// <param name="getDbSet">
        /// Method to retrieve dbset from context
        /// </param>
        /// <param name="sortFunction">
        /// Method to sort entities
        /// </param>
        /// <returns>The list of objects</returns>
        protected virtual async Task OnCollectionRequest<TObject>(
            CollectionRequest<TObject> collectionRequest,
            Func<TContext, DbSet<TObject>> getDbSet,
            Func<IQueryable<TObject>, IOrderedQueryable<TObject>> sortFunction)
            where TObject : class
        {
            using (var ds = await this.GetContext())
            {
                var query = sortFunction(getDbSet(ds)).Skip(collectionRequest.Skip);
                if (collectionRequest.Count.HasValue)
                {
                    query = query.Take(collectionRequest.Count.Value);
                }

                this.Sender.Tell(await query.ToListAsync());
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
        /// <param name="getDbSet">
        /// Method to retrieve dbset from context
        /// </param>
        /// <param name="getId">
        /// Method to retrieve id from object
        /// </param>
        /// <param name="getIdValidationExpression">
        /// Method to get expression to check object's id
        /// </param>
        /// <returns>
        /// Execution task
        /// </returns>
        protected virtual async Task OnRequest<TObject, TId>(
            RestActionMessage<TObject, TId> request,
             Func<TContext, DbSet<TObject>> getDbSet,
             Func<TObject, TId> getId,
             Func<TId, Expression<Func<TObject, bool>>> getIdValidationExpression)
            where TObject : class
        {
            using (var ds = await this.GetContext())
            {
                var set = getDbSet(ds);
                switch (request.ActionType)
                {
                    case EnActionType.Get:
                        this.Sender.Tell(this.OnSelect(await set.FirstOrDefaultAsync(getIdValidationExpression(request.Id))));
                        break;

                    case EnActionType.Create:
                        {
                            var entity = request.Request;
                            if (entity == null)
                            {
                                Context.GetLogger().Error("{Type}: create failed, no data", this.GetType().Name);
                                this.Sender.Tell(null);
                                return;
                            }

                            var oldObject = await set.FirstOrDefaultAsync(getIdValidationExpression(getId(entity)));
                            if (oldObject != null)
                            {
                                Context.GetLogger().Error("{Type}: create failed, there is already object with id {Id}", this.GetType().Name, getId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }

                            entity = this.BeforeCreate(entity);

                            if (entity == null)
                            {
                                Context.GetLogger().Error("{Type}: create failed, prevented by BeforeCreate", this.GetType().Name);
                                this.Sender.Tell(null);
                                return;
                            }

                            set.Add(entity);
                            try
                            {
                                await ds.SaveChangesAsync();
                                this.Sender.Tell(entity);
                                this.AfterCreate(entity);
                                return;
                            }
                            catch (Exception exception)
                            {
                                Context.GetLogger().Error(exception, "{Type}: create failed, error while creating object with id {Id}", this.GetType().Name, getId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }
                        }

                    case EnActionType.Update:
                        {
                            var entity = request.Request;
                            if (entity == null)
                            {
                                Context.GetLogger().Error("{Type}: create failed, no data", this.GetType().Name);
                                this.Sender.Tell(null);
                                return;
                            }

                            var oldObject = await set.FirstOrDefaultAsync(getIdValidationExpression(getId(entity)));
                            if (oldObject == null)
                            {
                                Context.GetLogger().Error("{Type}: update failed, there is no object with id {Id}", this.GetType().Name, getId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }

                            entity = this.BeforeUpdate(entity, oldObject);
                            if (entity == null)
                            {
                                Context.GetLogger().Error("{Type}: update of object with id {Id} failed, prevented by BeforeUpdate", this.GetType().Name, getId(oldObject).ToString());
                                this.Sender.Tell(null);
                                return;
                            }

                            set.Attach(entity);
                            try
                            {
                                await ds.SaveChangesAsync();
                                this.Sender.Tell(entity);
                                this.AfterUpdate(entity, oldObject);
                                return;
                            }
                            catch (Exception exception)
                            {
                                Context.GetLogger().Error(exception, "{Type}: update failed, error while creating object with id {Id}", this.GetType().Name, getId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }
                        }

                    case EnActionType.Delete:
                        var deletedObjects = await set.Where(getIdValidationExpression(request.Id)).ToListAsync();
                        set.RemoveRange(this.BeforeDelete(deletedObjects));
                        var success = await ds.SaveChangesAsync() > 0;
                        if (success)
                        {
                            this.AfterDelete(deletedObjects);
                        }

                        this.Sender.Tell(success);
                        break;

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