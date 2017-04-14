// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the BaseContextFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    /// Base factory to create entity framework contexts
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    /// <remarks>
    /// Expected that TContext has a public constructor with <see cref="DbConnection"/> and <see cref="bool"/> arguments
    /// </remarks>
    [UsedImplicitly]
    public class BaseContextFactory<TContext> : IContextFactory<TContext> where TContext : DbContext
    {
        /// <summary>
        /// Pre-compiled activator to create data context
        /// </summary>
        public static readonly Func<DbConnection, bool, TContext> Creator;

        /// <summary>
        /// Initializes static members of the <see cref="BaseContextFactory{TContext}"/> class.
        /// </summary>
        static BaseContextFactory()
        {
            var constructor = typeof(TContext).GetConstructor(new[] { typeof(DbConnection), typeof(bool) });

            if (constructor == null)
            {
                return;
            }

            var existingConnectionParameter = Expression.Parameter(typeof(DbConnection), "existingConnection");
            var contextOwnsConnectionParameter = Expression.Parameter(typeof(bool), "contextOwnsConnection");

            var newExp = Expression.New(constructor, existingConnectionParameter, contextOwnsConnectionParameter);

            var lambda = Expression.Lambda(
                typeof(Func<DbConnection, bool, TContext>),
                newExp,
                existingConnectionParameter,
                contextOwnsConnectionParameter);
            Creator = (Func<DbConnection, bool, TContext>)lambda.Compile();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseContextFactory{TContext}"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public BaseContextFactory([NotNull] BaseConnectionManager connectionManager)
        {
            if (connectionManager == null)
            {
                throw new ArgumentNullException(nameof(connectionManager));
            }

            this.ConnectionManager = connectionManager;
        }

        /// <summary>
        ///  Gets the connection manager.
        /// </summary>
        protected BaseConnectionManager ConnectionManager { get; }

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
        public virtual async Task<TContext> CreateContext(string connectionString, string databaseName)
        {
            databaseName = this.ConnectionManager.EscapeDatabaseName(databaseName);
            var connection = this.ConnectionManager.CreateConnection(connectionString);
            await connection.OpenAsync();

            this.ConnectionManager.SwitchDatabase(connection, databaseName);

            var context = Creator(connection, true);
            return context;
        }
    }
}