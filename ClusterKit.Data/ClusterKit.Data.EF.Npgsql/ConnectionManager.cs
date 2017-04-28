// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Npgsql connection manager
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Data.EF.Npgsql
{
    using System;
    using System.Data;
    using System.Data.Common;

    using global::Npgsql;

    using Serilog;

    /// <summary>
    /// Npgsql connection manager
    /// </summary>
    public class ConnectionManager : BaseConnectionManager
    {
        /// <inheritdoc />
        public override string ProviderInvariantName => "Npgsql";

        /// <inheritdoc />
        public override bool CheckDatabaseExistence(DbConnection connection, string databaseName)
        {
            var npgsqlConnection = connection as NpgsqlConnection;

            if (npgsqlConnection == null)
            {
                throw new ArgumentException("connection should be NpgsqlConnection");
            }

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                throw new ArgumentNullException(nameof(databaseName));
            }

            databaseName = this.EscapeDatabaseName(databaseName);
            var command = npgsqlConnection.CreateCommand();
            command.CommandText = $"SELECT count(*) FROM pg_database WHERE datname = '{databaseName}'";
            command.CommandType = CommandType.Text;

            var scalar = command.ExecuteScalar();
            if (scalar == null)
            {
                throw new Exception("Unexpected database behavior");
            }

            var count = (long)scalar;
            return count != 0;
        }

        /// <summary>
        /// Checks for database existence. In case it is not - creates it
        /// </summary>
        /// <param name="connection">Opened database connection</param>
        /// <param name="databaseName">Database name to check</param>
        public override void CheckCreateDatabase(DbConnection connection, string databaseName)
        {
            if (this.CheckDatabaseExistence(connection, databaseName))
            {
                return;
            }

            var npgsqlConnection = connection as NpgsqlConnection;
            if (npgsqlConnection == null)
            {
                throw new ArgumentException("connection should be NpgsqlConnection");
            }

            Log.Logger.Debug("{Type}: Creating database \"{DatabaseName}\"", this.GetType().FullName, databaseName);
            var command = npgsqlConnection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{databaseName}\"";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Creates connection to database
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        /// <returns>Database connection</returns>
        /// <remarks>Don't forget to dispose it</remarks>
        public override DbConnection CreateConnection(string connectionString)
        {
            var connection = new NpgsqlConnection(connectionString);
            return connection;
        }
    }
}