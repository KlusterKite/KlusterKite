// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UniversalContextFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   The context factory that searches the end-type context factory by provider name
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;

    using Autofac;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// The context factory that searches the end-type context factory by provider name
    /// </summary>
    public class UniversalContextFactory
    {
        /// <summary>
        /// The list of registered factories
        /// </summary>
        private readonly ImmutableDictionary<string, IContextFactory> factories;

        /// <summary>
        /// Initializes a new instance of the <see cref="UniversalContextFactory"/> class.
        /// </summary>
        /// <param name="componentContext">
        /// The windsor container.
        /// </param>
        public UniversalContextFactory(IComponentContext componentContext)
        {
            this.factories = componentContext.Resolve<IEnumerable<IContextFactory>>().ToImmutableDictionary(f => f.ProviderName);
        }

        /// <summary>
        /// Creates a new context
        /// </summary>
        /// <typeparam name="TContext">The type of context</typeparam>
        /// <param name="providerName">The database provider name</param>
        /// <param name="connectionString">The database connection string</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>The new context</returns>
        public TContext CreateContext<TContext>(string providerName, string connectionString, string databaseName = null)
            where TContext : DbContext
        {
            IContextFactory factory;
            if (!this.factories.TryGetValue(providerName, out factory))
            {
                throw new InvalidOperationException($"IContextFactory with provider name {providerName} is not registered");
            }

            return factory.CreateContext<TContext>(connectionString, databaseName);
        }
    }
}
