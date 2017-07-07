// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PostgresContextFactory.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Creates the new context factory
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.Npgsql
{
    using Microsoft.EntityFrameworkCore;
    using global::Npgsql;

    /// <summary>
    /// Creates the new context with Postgres connection
    /// </summary>
    public class PostgresContextFactory : BaseContextFactory
    {
        /// <inheritdoc />
        public override string ProviderName => "Npgsql";

        /// <inheritdoc />
        protected override DbContextOptions<TContext> GetContextOptions<TContext>(string connectionString, string databaseName)
        {
            if (!string.IsNullOrWhiteSpace(databaseName))
            {
                var connectionStringBuilder =
                    new NpgsqlConnectionStringBuilder(connectionString) { Database = databaseName };
                connectionString = connectionStringBuilder.ConnectionString;
            }

            var optionsBuilder = new DbContextOptionsBuilder<TContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return optionsBuilder.Options;
        }
    }
}
