// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReleaseDataFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with <see cref="Release" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.NodeManager.ConfigurationSource
{
    using System;
    using System.Linq.Expressions;

    using KlusterKite.Data.EF;
    using KlusterKite.NodeManager.Client.ORM;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data factory to work with <see cref="Release"/>
    /// </summary>
    public class ReleaseDataFactory : EntityDataFactory<ConfigurationContext, Release, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseDataFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// The current data source context
        /// </param>
        public ReleaseDataFactory(ConfigurationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The object's identification</returns>
        public override int GetId(Release obj)
            => obj.Id;

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        public override Expression<Func<Release, bool>> GetIdValidationExpression(int id)
            => t => t.Id == id;

        /// <summary>
        /// Gets the data set from current context
        /// </summary>
        /// <returns>The data set</returns>
        protected override DbSet<Release> GetDbSet() => this.Context.Releases;
    }
}
