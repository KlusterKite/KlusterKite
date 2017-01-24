// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NugetFeedFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="NodeTemplate" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;

    using ClusterKit.Data.EF;

    /// <summary>
    /// Data factory to work with <see cref="NodeTemplate"/>
    /// </summary>
    public class NugetFeedFactory : EntityDataFactory<ConfigurationContext, NugetFeed, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NugetFeedFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// The current datasource context
        /// </param>
        public NugetFeedFactory(ConfigurationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The object's identification</returns>
        public override int GetId(NugetFeed obj)
            => obj.Id;

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        public override Expression<Func<NugetFeed, bool>> GetIdValidationExpression(int id)
            => t => t.Id == id;

        /// <summary>
        /// Gets sort function to get an ordered list from datasource
        /// </summary>
        /// <param name="set">The unordered set of objects</param>
        /// <returns>The ordered set of objects</returns>
        public override IOrderedQueryable<NugetFeed> GetSortFunction(IQueryable<NugetFeed> set)
            => set.OrderBy(t => t.Id);

        /// <summary>
        /// Gets the dataset from current context
        /// </summary>
        /// <returns>The dataset</returns>
        protected override DbSet<NugetFeed> GetDbSet() => this.Context.NugetFeeds;
    }
}