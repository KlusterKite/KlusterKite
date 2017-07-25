// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationDataFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Data factory to work with Configuration
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
    /// Data factory to work with <see cref="Configuration"/>
    /// </summary>
    public class ConfigurationDataFactory : EntityDataFactory<ConfigurationContext, Configuration, int>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationDataFactory"/> class.
        /// </summary>
        /// <param name="context">
        /// The current data source context
        /// </param>
        public ConfigurationDataFactory(ConfigurationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Gets the object's identification from object
        /// </summary>
        /// <param name="obj">The data object</param>
        /// <returns>The object's identification</returns>
        public override int GetId(Configuration obj)
            => obj.Id;

        /// <summary>
        /// Gets the expression to check an object's identification
        /// </summary>
        /// <param name="id">The identification to check</param>
        /// <returns>The expression</returns>
        public override Expression<Func<Configuration, bool>> GetIdValidationExpression(int id)
            => t => t.Id == id;

        /// <summary>
        /// Gets the data set from current context
        /// </summary>
        /// <returns>The data set</returns>
        protected override DbSet<Configuration> GetDbSet() => this.Context.Configurations;
    }
}
