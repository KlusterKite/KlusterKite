using System;
using System.Linq;

namespace ClusterKit.NodeManager.ConfigurationSource
{
    using System.Data.Entity;
    using System.Linq.Expressions;

    using ClusterKit.Core.EF;

    using JetBrains.Annotations;

    /// <summary>
    /// Data factory to work with <see cref="NodeTemplate"/>
    /// </summary>
    [UsedImplicitly]
    public class SeedAddressFactorycs : EntityDataFactory<ConfigurationContext, SeedAddress, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeedAddressFactorycs"/> class.
        /// </summary>
        /// <param name="context">
        /// The current datasource context
        /// </param>
        public SeedAddressFactorycs(ConfigurationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The object's identification</returns>
        public override int GetId(SeedAddress obj)
            => obj.Id;

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        public override Expression<Func<SeedAddress, bool>> GetIdValidationExpression(int id)
            => t => t.Id == id;

        /// <summary>
        /// Gets sort function to get an ordered list from datasource
        /// </summary>
        /// <param name="set">The unordered set of objects</param>
        /// <returns>The ordered set of objects</returns>
        public override IOrderedQueryable<SeedAddress> GetSortFunction(IQueryable<SeedAddress> set)
            => set.OrderBy(t => t.Id);

        /// <summary>
        /// Gets the dataset from current context
        /// </summary>
        /// <returns>The dataset</returns>
        protected override DbSet<SeedAddress> GetDbSet() => this.Context.SeedAddresses;
    }
}