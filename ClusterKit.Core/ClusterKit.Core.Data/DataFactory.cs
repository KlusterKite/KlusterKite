// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DataFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the DataFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Data
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using Castle.Windsor;

    using ClusterKit.Core.Monads;

    using JetBrains.Annotations;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// Base factory to work with data objects
    /// </summary>
    /// <typeparam name="TContext">The current datasource context</typeparam>
    /// <typeparam name="TObject">Type of data object to work with</typeparam>
    /// <typeparam name="TId">The type of object identification field</typeparam>
    public abstract class DataFactory<TContext, TObject, TId> where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataFactory{TContext,TObject,TId}"/> class.
        /// </summary>
        /// <param name="context">
        /// The current datasource context.
        /// </param>
        protected DataFactory(TContext context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Gets the current datasource context
        /// </summary>
        [UsedImplicitly]
        protected TContext Context { get; }

        /// <summary>
        /// Gets the new data factory registered in DI
        /// </summary>
        /// <param name="context">Current data context</param>
        /// <returns>The new data factory</returns>
        public static DataFactory<TContext, TObject, TId> CreateFactory(TContext context)
        {
            var container = ServiceLocator.Current.GetInstance<IWindsorContainer>();
            return container.Resolve<DataFactory<TContext, TObject, TId>>(new { context });
        }

        /// <summary>
        /// Deletes object from datasource
        /// </summary>
        /// <param name="id">Objects identification</param>
        /// <returns>Removed objects data</returns>
        public abstract Task<Maybe<TObject>> Delete(TId id);

        /// <summary>
        /// Gets an object from datasource using it's identification
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
        /// Gets a list of objects from datasource
        /// </summary>
        /// <param name="skip">The number of objects to skip from select</param>
        /// <param name="count">The maximum number of objects to return. Returns all on null.</param>
        /// <returns>The list of objects from datasource</returns>
        public abstract Task<List<TObject>> GetList(int skip, int? count);

        /// <summary>
        /// Adds an object to datasource
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>Async execution task</returns>
        public abstract Task Insert(TObject obj);

        /// <summary>
        /// Updates an object in datasource
        /// </summary>
        /// <param name="newData">The new object's data</param>
        /// <param name="oldData">The old object's data</param>
        /// <returns>Async execution task</returns>
        public abstract Task Update(TObject newData, TObject oldData);
    }
}