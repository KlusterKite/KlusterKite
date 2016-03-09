// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConnectionManager.cs" company="ClusterKit">
//   All rights reserved
// </copyright>
// <summary>
//   Npgsql connection manager
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ClusterKit.Core.EF.Npgsql
{
    using System;
    using System.Data;
    using System.Data.Common;
    using System.Text.RegularExpressions;

    using global::Npgsql;

    /// <summary>
    /// Npgsql connection manager
    /// </summary>
    public class ConnectionManager : BaseConnectionManager
    {
        /// <summary>
        /// Checks for database existence. In case it is not - creates it
        /// </summary>
        /// <param name="connection">Opened database connection</param>
        /// <param name="databaseName">Database name to check</param>
        public override void CheckCreateDatabase(DbConnection connection, string databaseName)
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

            var command = npgsqlConnection.CreateCommand();
            databaseName = Regex.Replace(databaseName, "[^\\w]+", string.Empty);
            command.CommandText =
                $@"DO
                $do$
                BEGIN

                IF NOT EXISTS(SELECT 1 FROM pg_database WHERE datname = '{databaseName}') THEN
                      PERFORM dblink_exec('dbname=' || current_database(), 'CREATE DATABASE {databaseName}');
                END IF;

                END
                $do $";
            command.CommandType = CommandType.Text;

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
            return new NpgsqlConnection(connectionString);
        }
    }
}