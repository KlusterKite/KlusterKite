// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IContextFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Data context creation factory contract
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF
{
    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Data context creation factory contract
    /// </summary>
    [UsedImplicitly]
    public interface IContextFactory
    {
        /// <summary>
        /// Gets a unique provider name
        /// </summary>
        string ProviderName { get; }

        /// <summary>
        /// Creates context attached to data source.
        /// Data source will be used as is.
        /// </summary>
        /// <typeparam name="TContext">Type of context to create</typeparam>
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
        TContext CreateContext<TContext>(string connectionString, string databaseName = null) where TContext : DbContext;
    }
}