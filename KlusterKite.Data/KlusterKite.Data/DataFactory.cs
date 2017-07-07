// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DataFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using Autofac;

    using KlusterKite.API.Client;
    using KlusterKite.Core.Monads;
    using KlusterKite.Data.CRUD.ActionMessages;

    using JetBrains.Annotations;

    /// <summary>
    /// Base factory to work with data objects
    /// </summary>
    /// <typeparam name="TContext">The current data source context</typeparam>
    /// <typeparam name="TObject">Type of data object to work with</typeparam>
    /// <typeparam name="TId">The type of object identification field</typeparam>
    public abstract class DataFactory<TContext, TObject, TId> where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFactory{TContext,TObject,TId}"/> class.
        /// </summary>
        /// <param name="context">
        /// The current data source context.
        /// </param>
        protected DataFactory(TContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Gets the current data source context
        /// </summary>
        [UsedImplicitly]
        protected TContext Context { get; }

        /// <summary>
        /// Gets the new data factory registered in DI
        /// </summary>
        /// <param name="componentContext">
        /// The component Context.
        /// </param>
        /// <param name="context">
        /// Current data context
        /// </param>
        /// <returns>
        /// The new data factory
        /// </returns>
        public static DataFactory<TContext, TObject, TId> CreateFactory(IComponentContext componentContext, TContext context)
        {
            return componentContext.Resolve<DataFactory<TContext, TObject, TId>>(new TypedParameter(typeof(TContext), context));
        }

        /// <summary>
        /// Deletes object from data source
        /// </summary>
        /// <param name="id">Objects identification</param>
        /// <returns>Removed objects data</returns>
        public abstract Task<Maybe<TObject>> Delete(TId id);

        /// <summary>
        /// Gets an object from data source using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>Async execution task</returns>
        public abstract Task<Maybe<TObject>> Get(TId id);

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The object's identification</returns>
        public abstract TId GetId(TObject obj);

        /// <summary>
        /// Gets a list of objects from data source
        /// </summary>
        /// <param name="filter">
        /// The filter condition.
        /// </param>
        /// <param name="sort">
        /// The sort condition.
        /// </param>
        /// <param name="skip">
        /// The number of objects to skip from select
        /// </param>
        /// <param name="count">
        /// The maximum number of objects to return. Returns all on null.
        /// </param>
        /// <param name="apiRequest">
        /// The original <see cref="ApiRequest"/>. Optional.
        /// </param>
        /// <returns>
        /// The list of objects from data source
        /// </returns>
        public abstract Task<CollectionResponse<TObject>> GetList(
            Expression<Func<TObject, bool>> filter,
            List<SortingCondition> sort,
            int? skip, 
            int? count,
            ApiRequest apiRequest);

        /// <summary>
        /// Adds an object to data source
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>Async execution task</returns>
        public abstract Task Insert(TObject obj);

        /// <summary>
        /// Updates an object in data source
        /// </summary>
        /// <param name="newData">The new object's data</param>
        /// <param name="oldData">The old object's data</param>
        /// <returns>Async execution task</returns>
        public abstract Task Update(TObject newData, TObject oldData);
    }
}