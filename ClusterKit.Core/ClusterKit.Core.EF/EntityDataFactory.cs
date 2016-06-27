// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityDataFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base factory to work with data objects using Entity Framework
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.EF
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.Core.Data;
    using ClusterKit.Core.Monads;

    using JetBrains.Annotations;

    /// <summary>
    /// Base factory to work with data objects using Entity Framework
    /// </summary>
    /// <typeparam name="TContext">The current datasource context</typeparam>
    /// <typeparam name="TObject">Type of data object to work with</typeparam>
    /// <typeparam name="TId">The type of object identification field</typeparam>
    [UsedImplicitly]
    public abstract class EntityDataFactory<TContext, TObject, TId> : DataFactory<TContext, TObject, TId>
        where TObject : class
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDataFactory{TContext,TObject,TId}"/> class.
        /// </summary>
        /// <param name="context">
        /// The current datasource context
        /// </param>
        protected EntityDataFactory(TContext context) : base(context)
        {
        }

        /// <summary>
        /// Deletes object from datasource
        /// </summary>
        /// <param name="id">Objects identification</param>
        /// <returns>Removed objects data</returns>
        public override async Task<Maybe<TObject>> Delete(TId id)
        {
            var oldObject = await this.Get(id);
            if (oldObject == null)
            {
                return null;
            }

            this.GetDbSet().Remove(oldObject);
            await this.Context.SaveChangesAsync();
            return oldObject;
        }

        /// <summary>
        /// Gets an object from datasource using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>Async execution task</returns>
        public override async Task<Maybe<TObject>> Get(TId id)
        {
            return await this.GetDbQuery().FirstOrDefaultAsync(this.GetIdValidationExpression(id));
        }

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        [UsedImplicitly]
        public abstract Expression<Func<TObject, bool>> GetIdValidationExpression(TId id);

        /// <summary>
        /// Gets a list of objects from datasource
        /// </summary>
        /// <param name="skip">The number of objects to skip from select</param>
        /// <param name="count">The maximum number of objects to return. Returns all on null.</param>
        /// <returns>The list of objects from datasource</returns>
        public override async Task<List<TObject>> GetList(int skip, int? count)
        {
            var query = this.GetSortFunction(this.GetDbQuery()).Skip(skip);
            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets sort function to get an ordered list from datasource
        /// </summary>
        /// <param name="set">The unordered set of objects</param>
        /// <returns>The ordered set of objects</returns>
        [UsedImplicitly]
        public abstract IOrderedQueryable<TObject> GetSortFunction(IQueryable<TObject> set);

        /// <summary>
        /// Adds an object to datasource
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>Async execution task</returns>
        public override async Task Insert(TObject obj)
        {
            this.GetDbSet().Add(obj);
            await this.Context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an object in datasource
        /// </summary>
        /// <param name="newData">The new object's data</param>
        /// <param name="oldData">The old object's data</param>
        /// <returns>Async execution task</returns>
        public override async Task Update(TObject newData, TObject oldData)
        {
            this.Context.Entry(oldData).CurrentValues.SetValues(newData);
            await this.Context.SaveChangesAsync();
        }

        /// <summary>
        /// Gets the query to receive all objects
        /// </summary>
        /// <returns>The query</returns>
        protected virtual DbQuery<TObject> GetDbQuery() => this.GetDbSet();

        /// <summary>
        /// Gets the dataset from current context
        /// </summary>
        /// <returns>The dataset</returns>
        protected abstract DbSet<TObject> GetDbSet();
    }
}