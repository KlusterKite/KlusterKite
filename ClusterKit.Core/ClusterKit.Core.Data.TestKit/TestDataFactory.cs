// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestDataFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Base mock object for data access
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.Data.TestKit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using ClusterKit.Core.Monads;

    using JetBrains.Annotations;

    /// <summary>
    /// Base mock object for data access
    /// </summary>
    /// <typeparam name="TContext">The current datasource context</typeparam>
    /// <typeparam name="TObject">Type of data object to work with</typeparam>
    /// <typeparam name="TId">The type of object identification field</typeparam>
    /// <remarks>This mock should be installed in DI as singleton (because it stores data in it's field)</remarks>
    public abstract class TestDataFactory<TContext, TObject, TId>
                        : DataFactory<TContext, TObject, TId>
                where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestDataFactory{TContext,TObject,TId}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        protected TestDataFactory(TContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Local mock storage
        /// </summary>
        [UsedImplicitly]
        public Dictionary<TId, TObject> Storage { get; } = new Dictionary<TId, TObject>();

        /// <summary>
        /// Deletes object from datasource
        /// </summary>
        /// <param name="id">Objects identification</param>
        /// <returns>Removed objects data</returns>
        public override Task<Maybe<TObject>> Delete(TId id)
        {
            TObject obj;
            if (this.Storage.TryGetValue(id, out obj))
            {
                this.Storage.Remove(id);
                return Task.FromResult(new Maybe<TObject>(obj));
            }

            return Task.FromResult(new Maybe<TObject>(null));
        }

        /// <summary>
        /// Gets an object from datasource using it's identification
        /// </summary>
        /// <param name="id">The object's identification</param>
        /// <returns>Async execution task</returns>
        public override Task<Maybe<TObject>> Get(TId id)
        {
            TObject obj;
            return this.Storage.TryGetValue(id, out obj) ? Task.FromResult(new Maybe<TObject>(obj)) : Task.FromResult(new Maybe<TObject>(null));
        }

        /// <summary>
        /// Gets a list of objects from datasource
        /// </summary>
        /// <param name="skip">The number of objects to skip from select</param>
        /// <param name="count">The maximum number of objects to return. Returns all on null.</param>
        /// <returns>The list of objects from datasource</returns>
        public override Task<List<TObject>> GetList(int skip, int? count)
        {
            var objects = this.Storage.Values.Skip(skip);
            if (count.HasValue)
            {
                objects = objects.Take(count.Value);
            }

            return Task.FromResult(objects.ToList());
        }

        /// <summary>
        /// Adds an object to datasource
        /// </summary>
        /// <param name="obj">The object to add</param>
        /// <returns>Async execution task</returns>
        public override Task Insert(TObject obj)
        {
            if (this.Storage.ContainsKey(this.GetId(obj)))
            {
                throw new InvalidOperationException("Duplicate insert");
            }

            this.Storage[this.GetId(obj)] = obj;
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Updates an object in datasource
        /// </summary>
        /// <param name="newData">The new object's data</param>
        /// <param name="oldData">The old object's data</param>
        /// <returns>Async execution task</returns>
        public override Task Update(TObject newData, TObject oldData)
        {
            if (!this.Storage.ContainsKey(this.GetId(oldData)))
            {
                throw new InvalidOperationException("There is no item to update");
            }

            if (!this.GetId(oldData).Equals(this.GetId(newData)))
            {
                this.Storage.Remove(this.GetId(oldData));
            }

            this.Storage[this.GetId(newData)] = newData;
            return Task.FromResult<object>(null);
        }
    }
}