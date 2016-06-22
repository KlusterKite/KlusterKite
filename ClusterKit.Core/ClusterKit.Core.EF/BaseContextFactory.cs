// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the BaseContextFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.EF
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using ClusterKit.Core.Data;

    using JetBrains.Annotations;

    /// <summary>
    /// Base factory to create entity framework contexts
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    /// <typeparam name="TMigrationConfiguration">
    /// Database migration configuration
    /// </typeparam>
    /// <remarks>
    /// Expected that TContext has a public constructor with <see cref="DbConnection"/> and <see cref="bool"/> arguments
    /// </remarks>
    public class BaseContextFactory<TContext, TMigrationConfiguration>
        : IContextFactory<TContext>
         where TMigrationConfiguration : DbMigrationsConfiguration<TContext>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Precompiled activator to create data context
        /// </summary>
        public static readonly Func<DbConnection, bool, TContext> Creator;

        /// <summary>
        ///  The connection manager.
        /// </summary>
        private readonly BaseConnectionManager connectionManager;

        /// <summary>
        /// Initializes static members of the <see cref="BaseContextFactory{TContext, TMigrationConfiguration}"/> class.
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
        /// Initializes a new instance of the <see cref="BaseContextFactory{TContext, TMigrationConfiguration}"/> class.
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

            this.connectionManager = connectionManager;
        }

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
        public virtual async Task<TContext> CreateAndUpgradeContext(
            [NotNull] string connectionString,
            [NotNull] string databaseName)
        {
            if (connectionString == null)
            {
                throw new ArgumentNullException(nameof(connectionString));
            }

            if (databaseName == null)
            {
                throw new ArgumentNullException(nameof(databaseName));
            }

            if (this.connectionManager == null)
            {
                throw new Exception(@"connectionManager is null");
            }

            databaseName = this.connectionManager.EscapeDatabaseName(databaseName);
            var connection = this.connectionManager.CreateConnection(connectionString);

            if (connection == null)
            {
                throw new Exception(@"Created null connection");
            }

            await connection.OpenAsync();
            this.connectionManager.CheckCreateDatabase(connection, databaseName);
            connection.ChangeDatabase(databaseName);

            if (Creator == null)
            {
                throw new Exception(@"Creator is null");
            }

            var context = Creator(connection, true);

            if (context == null)
            {
                throw new Exception(@"Created null context");
            }

            try
            {
                var migrator = new MigrateDatabaseToLatestVersion<TContext, TMigrationConfiguration>(true);
                migrator.InitializeDatabase(context);
            }
            catch (DbEntityValidationException entityValidationException)
            {
                var entityErrors = string.Join(
                    "",
                    entityValidationException.EntityValidationErrors.Select(error => $"\t{error.Entry.GetType().Name}:\n{string.Join("", error.ValidationErrors.Select(ve => $"\t\t{ve.PropertyName}: {ve.ErrorMessage}\n"))}"));
                var errorMessage = $"{typeof(TContext).Name} migration error\n{entityErrors}";
                context.Dispose();
                throw new Exception(errorMessage, entityValidationException);
            }
            catch (Exception)
            {
                context.Dispose();
                throw;
            }

            return context;
        }

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
        public virtual async Task<TContext> CreateContext(string connectionString, string databaseName)
        {
            databaseName = this.connectionManager.EscapeDatabaseName(databaseName);
            var connection = this.connectionManager.CreateConnection(connectionString);
            await connection.OpenAsync();
            connection.ChangeDatabase(databaseName);
            var context = Creator(connection, true);
            return context;
        }
    }
}