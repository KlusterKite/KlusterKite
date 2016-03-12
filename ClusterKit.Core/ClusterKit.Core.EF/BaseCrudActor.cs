// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCrudActor.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Generic actor to permom basic crud operation on EF objects
// </summary>
// --------------------------------------------------------------------------------------------------------------------

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
    /// <typeparam name="TObject">
    /// The type of ef object
    /// </typeparam>
    /// <typeparam name="TId">
    /// The type of object identity field
    /// </typeparam>
    public abstract class BaseCrudActor<TContext, TObject, TId>
        : ReceiveActor
        where TContext : DbContext
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCrudActor{TContext,TObject,TId}"/> class.
        /// </summary>
        protected BaseCrudActor()
        {
            this.Receive<RestActionMessage<TObject, TId>>(m => this.OnRequest(m));
        }

        /// <summary>
        /// Method called after successful object creation in database
        /// </summary>
        /// <param name="result">Created object</param>
        protected virtual void AfterCreate(TObject result)
        {
        }

        /// <summary>
        /// Method called after successful object removal from database
        /// </summary>
        /// <param name="deletedObjects">List of removed objects</param>
        protected void AfterDelete(List<TObject> deletedObjects)
        {
        }

        /// <summary>
        /// Method called after successful object modification in database
        /// </summary>
        /// <param name="newObject">
        /// The new Object.
        /// </param>
        /// <param name="oldObject">
        /// The old Object.
        /// </param>
        protected virtual void AfterUpdate(TObject newObject, TObject oldObject)
        {
        }

        /// <summary>
        /// Method call before object creation in database
        /// </summary>
        /// <param name="request">Object intended to be created</param>
        /// <returns>Object that will be created or null to prevent creation</returns>
        protected virtual TObject BeforeCreate(TObject request)
        {
            return request;
        }

        /// <summary>
        /// Method call before object removal database
        /// </summary>
        /// <param name="deletedObjects">Object intended to be removed</param>
        /// <returns>Object that will be removed</returns>
        protected List<TObject> BeforeDelete(List<TObject> deletedObjects)
        {
            return deletedObjects;
        }

        /// <summary>
        /// Method called before object modification in database
        /// </summary>
        /// <param name="newObject">
        /// The new Object.
        /// </param>
        /// <param name="oldObject">
        /// The old Object.
        /// </param>
        /// <returns>
        /// The new version of object or null to prevent update
        /// </returns>
        protected virtual TObject BeforeUpdate(TObject newObject, TObject oldObject)
        {
            return newObject;
        }

        /// <summary>
        /// Gets current data context
        /// </summary>
        /// <returns>The data context</returns>
        protected abstract Task<TContext> GetContext();

        /// <summary>
        /// Gets the table from context, corresponding current data object
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        /// <returns>
        /// The table
        /// </returns>
        protected abstract DbSet<TObject> GetDbSet(TContext context);

        /// <summary>
        /// Gets an object id
        /// </summary>
        /// <param name="object">The data object</param>
        /// <returns>Object identification number</returns>
        protected abstract TId GetId(TObject @object);

        /// <summary>
        /// Gets expression to validate id in data object
        /// </summary>
        /// <param name="id">Object identification number</param>
        /// <returns>Validity of identification field</returns>
        protected abstract Expression<Func<TObject, bool>> GetIdValidationExpression(TId id);

        /// <summary>
        /// Called on select. Sender will receive this method output.
        /// </summary>
        /// <param name="result">Selected object from database</param>
        /// <returns>Result for requester</returns>
        protected virtual TObject OnSelect(TObject result)
        {
            return result;
        }

        /// <summary>
        /// Request process method
        /// </summary>
        /// <param name="request">The action request</param>
        /// <returns>Execution task</returns>
        private async Task OnRequest(RestActionMessage<TObject, TId> request)
        {
            using (var ds = await this.GetContext())
            {
                var set = this.GetDbSet(ds);
                switch (request.ActionType)
                {
                    case EnActionType.Get:
                        this.Sender.Tell(this.OnSelect(await set.FirstOrDefaultAsync(this.GetIdValidationExpression(request.Id))));
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

                            var oldObject = await set.FirstOrDefaultAsync(this.GetIdValidationExpression(this.GetId(entity)));
                            if (oldObject != null)
                            {
                                Context.GetLogger().Error("{Type}: create failed, there is already object with id {Id}", this.GetType().Name, this.GetId(entity).ToString());
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
                                Context.GetLogger().Error(exception, "{Type}: create failed, error while creating object with id {Id}", this.GetType().Name, this.GetId(entity).ToString());
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

                            var oldObject = await set.FirstOrDefaultAsync(this.GetIdValidationExpression(this.GetId(entity)));
                            if (oldObject == null)
                            {
                                Context.GetLogger().Error("{Type}: update failed, there is no object with id {Id}", this.GetType().Name, this.GetId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }

                            entity = this.BeforeUpdate(entity, oldObject);
                            if (entity == null)
                            {
                                Context.GetLogger().Error("{Type}: update of object with id {Id} failed, prevented by BeforeUpdate", this.GetType().Name, this.GetId(oldObject).ToString());
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
                                Context.GetLogger().Error(exception, "{Type}: update failed, error while creating object with id {Id}", this.GetType().Name, this.GetId(entity).ToString());
                                this.Sender.Tell(null);
                                return;
                            }
                        }
                    case EnActionType.Delete:
                        var deletedObjects = await set.Where(this.GetIdValidationExpression(request.Id)).ToListAsync();
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
    }
}