// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Data context creation factory contract
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data
{
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Data context creation factory contract
    /// </summary>
    /// <typeparam name="TContext">Type of context to create</typeparam>
    [UsedImplicitly]
    public interface IContextFactory<TContext>
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
        [UsedImplicitly]
        Task<TContext> CreateAndUpgradeContext(string connectionString, string databaseName);

        /// <summary>
        /// Creates context attached to data source.
        /// Data source will be used as is.
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
        [UsedImplicitly]
        Task<TContext> CreateContext(string connectionString, string databaseName);
    }
}