// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Mock object for <see cref="IContextFactory{TContext}" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.TestKit
{
    using System.Threading.Tasks;

    /// <summary>
    /// Mock object for <see cref="IContextFactory{TContext}"/>
    /// </summary>
    /// <typeparam name="TContext">Type of context to create</typeparam>
    /// <remarks>Just returns context from default constructor</remarks>
    public class TestContextFactory<TContext>
        : IContextFactory<TContext>
        where TContext : new()
    {
        /// <summary>
        /// Creates context attached to datasource.
        /// Datasource will be modified (database will be created, migrations will be run).
        /// </summary>
        /// <param name="connectionString">
        /// The connection String.
        /// </param>
        /// <param name="databaseName">
        /// The database Name.
        /// </param>
        /// <returns>
        /// The data context
        /// </returns>
        public Task<TContext> CreateAndUpgradeContext(string connectionString, string databaseName) => Task.FromResult(new TContext());

        /// <summary>
        /// Creates context attached to datasource.
        /// Datasource will be used as is.
        /// </summary>
        /// <param name="connectionString">
        /// The connection String.
        /// </param>
        /// <param name="databaseName">
        /// The database Name.
        /// </param>
        /// <returns>
        /// The data context
        /// </returns>
        public Task<TContext> CreateContext(string connectionString, string databaseName) => Task.FromResult(new TContext());
    }
}