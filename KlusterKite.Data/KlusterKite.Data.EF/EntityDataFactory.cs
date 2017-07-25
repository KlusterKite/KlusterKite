// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EntityDataFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Base factory to work with data objects using Entity Framework
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using KlusterKite.API.Client;
    using KlusterKite.Core.Monads;
    using KlusterKite.Data;
    using KlusterKite.Data.CRUD.ActionMessages;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Base factory to work with data objects using Entity Framework
    /// </summary>
    /// <typeparam name="TContext">The current data source context</typeparam>
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
        /// The current data source context
        /// </param>
        protected EntityDataFactory(TContext context) : base(context)
        {
        }

        /// <summary>
        /// Deletes object from data source
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
        /// Gets an object from data source using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>Async execution task</returns>
        public override async Task<Maybe<TObject>> Get(TId id)
        {
            return await this.GetDbSet().FirstOrDefaultAsync(this.GetIdValidationExpression(id));
        }

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        [UsedImplicitly]
        public abstract Expression<Func<TObject, bool>> GetIdValidationExpression(TId id);

        /// <inheritdoc />
        public override async Task<CollectionResponse<TObject>> GetList(
            Expression<Func<TObject, bool>> filter,
            List<SortingCondition> sort,
            int? skip,
            int? count,
            ApiRequest apiRequest)
        {
            var query = this.GetDbSet() as IQueryable<TObject>;
            
            if (apiRequest != null)
            {
                query = query.SetIncludes(this.Context, apiRequest);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            var result = new CollectionResponse<TObject> { Count = await query.CountAsync() };
            if (sort != null)
            {
                query = query.ApplySorting(sort);
            }

            if (skip.HasValue && query is IOrderedQueryable<TObject>)
            {
                query = query.Skip(skip.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            result.Items = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// Adds an object to data source
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>Async execution task</returns>
        public override async Task Insert(TObject obj)
        {
            this.GetDbSet().Add(obj);
            await this.Context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates an object in data source
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
        /// Gets the data set from current context
        /// </summary>
        /// <returns>The data set</returns>
        protected abstract DbSet<TObject> GetDbSet();
    }
}