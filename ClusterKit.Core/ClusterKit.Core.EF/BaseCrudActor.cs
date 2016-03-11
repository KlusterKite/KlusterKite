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
                        this.Sender.Tell(await set.FirstOrDefaultAsync(this.GetIdValidationExpression(request.Id)));
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

                            set.Add(entity);
                            try
                            {
                                await ds.SaveChangesAsync();
                                this.Sender.Tell(entity);
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

                            set.Attach(entity);
                            try
                            {
                                await ds.SaveChangesAsync();
                                this.Sender.Tell(entity);
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
                        set.RemoveRange(await set.Where(this.GetIdValidationExpression(request.Id)).ToListAsync());
                        this.Sender.Tell(await ds.SaveChangesAsync() > 0);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}