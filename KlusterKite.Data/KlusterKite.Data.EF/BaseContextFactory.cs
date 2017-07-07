// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseContextFactory.cs" company="KlusterKite">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the BaseContextFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace KlusterKite.Data.EF
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    using JetBrains.Annotations;

    using Microsoft.EntityFrameworkCore;

    /// <summary>
    /// Base factory to create entity framework contexts
    /// </summary>
    /// <remarks>
    /// Expected that TContext has a public constructor with <see cref="DbContextOptions{TContext}"/> argument
    /// </remarks>
    [UsedImplicitly]
    public abstract class BaseContextFactory : IContextFactory
    {
        /// <summary>
        /// Gets a unique provider name
        /// </summary>
        public abstract string ProviderName { get; }

        /// <summary>
        /// Creates a new instance of context class
        /// </summary>
        /// <typeparam name="TContext">The type of context</typeparam>
        /// <param name="options">
        /// The context options.
        /// </param>
        /// <returns>
        /// The new context
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The context class must have a public constructor that accepts <see cref="DbContextOptions{TContext}"/> as single parameter
        /// </exception>
        [UsedImplicitly]
        public static TContext CreateContext<TContext>(DbContextOptions<TContext> options)
            where TContext : DbContext
        {
            return ContextCreator<TContext>.Create(options);
        }

        /// <inheritdoc />
        public TContext CreateContext<TContext>(string connectionString, string databaseName = null)
            where TContext : DbContext
        {
            var options = this.GetContextOptions<TContext>(connectionString, databaseName);
            return CreateContext(options);
        }

        /// <summary>
        /// Creates a context options for a given data
        /// </summary>
        /// <typeparam name="TContext">The type of context</typeparam>
        /// <param name="connectionString">The connection string</param>
        /// <param name="databaseName">The database name</param>
        /// <returns>The context</returns>
        protected abstract DbContextOptions<TContext> GetContextOptions<TContext>(
            string connectionString,
            string databaseName) where TContext : DbContext;

        /// <summary>
        /// Helper to create new context instances
        /// </summary>
        /// <typeparam name="TContext">The type of context</typeparam>
        private static class ContextCreator<TContext>
            where TContext : DbContext
        {
            /// <summary>
            /// The pre-compiled creator method
            /// </summary>
            private static readonly Func<DbContextOptions<TContext>, TContext> Creator;

            /// <summary>
            /// Initializes static members of the <see cref="ContextCreator{TContext}"/> class.
            /// </summary>
            static ContextCreator()
            {
                var constructor = typeof(TContext).GetTypeInfo().GetConstructor(new[] { typeof(DbContextOptions<TContext>) });
                if (constructor == null)
                {
                    return;
                }

                var optionsParameter = Expression.Parameter(typeof(DbContextOptions<TContext>), "options");
                var newExp = Expression.New(constructor, optionsParameter);

                var lambda = Expression.Lambda(
                    typeof(Func<DbContextOptions<TContext>, TContext>),
                    newExp,
                    optionsParameter);

                Creator = (Func<DbContextOptions<TContext>, TContext>)lambda.Compile();
            }

            /// <summary>
            /// Creates a new instance of context class
            /// </summary>
            /// <param name="options">
            /// The context options.
            /// </param>
            /// <returns>
            /// The new context
            /// </returns>
            /// <exception cref="InvalidOperationException">
            /// The context class must have a public constructor that accepts <see cref="DbContextOptions{TContext}"/> as single parameter
            /// </exception>
            public static TContext Create(DbContextOptions<TContext> options)
            {
                if (Creator == null)
                {
                    throw new InvalidOperationException(
                        $"{typeof(TContext).FullName} does not have a public constructor "
                        + $"with single parameter of type DbContextOptions<{typeof(TContext).FullName}>");
                }

                return Creator(options);
            }
        }
    }
}