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
    using System.Data.Entity.Migrations;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    /// <summary>
    ///  Base factory to create entity framework contexts respecting Effort database engine
    /// </summary>
    /// <typeparam name="TContext">The type of context</typeparam>
    /// <typeparam name="TMigrationConfiguration">
    /// Database migration configuration
    /// </typeparam>
    public class EffortContextFactory<TContext, TMigrationConfiguration> : BaseContextFactory<TContext, TMigrationConfiguration>
        where TMigrationConfiguration : DbMigrationsConfiguration<TContext>, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EffortContextFactory{TContext,TMigrationConfiguration}"/> class.
        /// </summary>
        /// <param name="connectionManager">
        /// The connection manager.
        /// </param>
        public EffortContextFactory([NotNull] BaseConnectionManager connectionManager)
            : base(connectionManager)
        {
        }

        /// <inheritdoc />
        public override Task<TContext> CreateAndUpgradeContext(string connectionString, string databaseName)
        {
            return this.CreateContext(connectionString, databaseName);
        }

        /// <inheritdoc />
        public override Task<TContext> CreateContext(string connectionString, string databaseName)
        {
            var connection = this.ConnectionManager.CreateConnection(connectionString);
            return Task.FromResult(Creator(connection, true));
        }
    }
}
