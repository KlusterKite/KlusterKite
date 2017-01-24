// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniversalTestDataFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Universal class to mock basic data factories (with only CRUD operations)
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.TestKit
{
    using System;

    /// <summary>
    /// Universal class to mock basic data factories (with only CRUD operations)
    /// </summary>
    /// <typeparam name="TContext">The current datasource context</typeparam>
    /// <typeparam name="TObject">Type of data object to work with</typeparam>
    /// <typeparam name="TId">The type of object identification field</typeparam>
    /// <remarks>This mock should be installed in DI as singleton (because it stores data in it's field)</remarks>
    public class UniversalTestDataFactory<TContext, TObject, TId> : TestDataFactory<TContext, TObject, TId>
                    where TObject : class
    {
        /// <summary>
        /// The main mean to get identification value from data object
        /// </summary>
        private readonly Func<TObject, TId> getIdFunc;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalTestDataFactory{TContext,TObject,TId}"/> class.
        /// </summary>
        /// <param name="context">
        /// The data context.
        /// </param>
        /// <param name="getIdFunc">
        /// The main mean to get identification value from data object
        /// </param>
        public UniversalTestDataFactory(TContext context, Func<TObject, TId> getIdFunc)
                : base(context)
        {
            this.getIdFunc = getIdFunc;
        }

        /// <summary>
        /// Gets the identification value from data object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The identification value</returns>
        public override TId GetId(TObject obj)
        {
            return this.getIdFunc(obj);
        }
    }
}