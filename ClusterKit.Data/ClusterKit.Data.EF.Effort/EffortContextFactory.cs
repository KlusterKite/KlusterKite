// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EffortContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Defines the EffortContextFactory type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.Effort
{
    using System.Data.Entity;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    ///  Base factory to create entity framework contexts respecting Effort database engine
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    public class EffortContextFactory<TContext> : BaseContextFactory<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffortContextFactory{TContext}"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public EffortContextFactory([NotNull] BaseConnectionManager connectionManager)
            : base(connectionManager)
        {
        }

        /// <inheritdoc />
        public override Task<TContext> CreateContext(string connectionString, string databaseName)
        {
            var connection = this.ConnectionManager.CreateConnection(connectionString);
            return Task.FromResult(Creator(connection, true));
        }
    }
}
